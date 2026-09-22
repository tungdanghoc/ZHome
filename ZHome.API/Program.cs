using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ZHome.API.Data;
using ZHome.API.Models;
using ZHome.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Hỗ trợ nhận PORT động khi deploy trên Render / Cloud
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// 1. Add DB Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ZHomeDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Register Payment (SePay & PayOS) Settings & Services
builder.Services.Configure<SePaySettings>(builder.Configuration.GetSection("SePay"));
builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOS"));
builder.Services.AddSingleton<PaymentOrderStore>();
builder.Services.AddScoped<ISePayService, SePayService>();
builder.Services.AddScoped<IPayOSService, PayOSService>();

// 3. Register Custom Services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// 4. Configure Controllers and Routing
builder.Services.AddControllers();

// 5. Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "ZHome API", 
        Version = "v1",
        Description = "ZHome Boarding Management & Roommate Matching API"
    });
    
    // Add JWT authentication support in Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Nhập token JWT của bạn: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// 6. Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ZHome_SuperSecretKeyThatIsAtLeast32BytesLongForSecurity_2026";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ZHome.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ZHome.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// 7. Configure CORS (Cho phép cả Localhost & Production Frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Cho phép tất cả domain truy cập an toàn
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Auto-Migration: Tự động tạo/bổ sung bảng, cột thiếu (bao gồm cả cột 'level' trong bảng locations)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ZHomeDbContext>();
        dbContext.Database.ExecuteSqlRaw(@"
            -- 1. Bảng locations: Thêm cột level nếu chưa có
            IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'locations')
            BEGIN
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'locations' AND COLUMN_NAME = 'level')
                    ALTER TABLE [dbo].[locations] ADD [level] INT NOT NULL DEFAULT 1;
            END;

            -- 2. Bảng notifications
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'notifications')
            BEGIN
                CREATE TABLE [dbo].[notifications] (
                    [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [user_id] BIGINT NOT NULL,
                    [title] NVARCHAR(200) NOT NULL,
                    [message] NVARCHAR(MAX) NOT NULL,
                    [type] NVARCHAR(50) NOT NULL DEFAULT 'System',
                    [target_url] NVARCHAR(255) NULL,
                    [reference_id] BIGINT NULL,
                    [is_read] BIT NOT NULL DEFAULT 0,
                    [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT [FK_notifications_users] FOREIGN KEY ([user_id]) REFERENCES [dbo].[users]([id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_notifications_user_id] ON [dbo].[notifications]([user_id]);
                CREATE INDEX [IX_notifications_is_read] ON [dbo].[notifications]([is_read]);
            END;

            -- 3. Bảng matching_profiles
            IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'matching_profiles')
            BEGIN
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'matching_profiles' AND COLUMN_NAME = 'title')
                    ALTER TABLE matching_profiles ADD title NVARCHAR(255) NULL;
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'matching_profiles' AND COLUMN_NAME = 'university')
                    ALTER TABLE matching_profiles ADD university NVARCHAR(150) NULL;
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'matching_profiles' AND COLUMN_NAME = 'has_room')
                    ALTER TABLE matching_profiles ADD has_room BIT NOT NULL DEFAULT 0;
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'matching_profiles' AND COLUMN_NAME = 'address')
                    ALTER TABLE matching_profiles ADD address NVARCHAR(255) NULL;
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'matching_profiles' AND COLUMN_NAME = 'contact_phone')
                    ALTER TABLE matching_profiles ADD contact_phone NVARCHAR(50) NULL;
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'matching_profiles' AND COLUMN_NAME = 'image_url')
                    ALTER TABLE matching_profiles ADD image_url NVARCHAR(MAX) NULL;
            END;

            -- 4. Bảng monthly_bills
            IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'monthly_bills')
            BEGIN
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'monthly_bills' AND COLUMN_NAME = 'proof_image_url')
                    ALTER TABLE monthly_bills ADD proof_image_url NVARCHAR(500) NULL;

                IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'chk_bill_status')
                BEGIN
                    ALTER TABLE [dbo].[monthly_bills] DROP CONSTRAINT [chk_bill_status];
                    ALTER TABLE [dbo].[monthly_bills] ADD CONSTRAINT [chk_bill_status] CHECK ([status] IN ('Paid', 'Unpaid', 'Partial', 'PendingConfirmation', 'PartialPaid', 'Rejected'));
                END;
            END;
        ");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB Auto-Migration Warning] {ex.Message}");
    }
}

// Bật Swagger trên CẢ Production & Development để test API trực tiếp trên link Render
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZHome API v1");
    // Mở trang chủ Render sẽ vào thẳng giao diện Swagger UI
    c.RoutePrefix = string.Empty; 
});

app.UseStaticFiles();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
