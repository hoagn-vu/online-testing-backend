using System.Text;
using Backend_online_testing.Services;
using Backend_online_testing.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add configuration file
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Connection.json", optional: true, reloadOnChange: true);

// Configure MongoDb
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDbConnection");
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("onlineTestingDB");
});

builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<RoomsService>();
builder.Services.AddScoped<RoomRepository>();;
builder.Services.AddScoped<ExamsService>();
builder.Services.AddScoped<ExamRepository>();
builder.Services.AddSingleton<ExamMatricesService>();
builder.Services.AddSingleton<AddLogService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<LogRepository>();
builder.Services.AddScoped<GroupUserRepository>();
builder.Services.AddScoped<GroupUserService>();
builder.Services.AddSingleton<FileManagementService>();
builder.Services.AddScoped<IFileManagementService, FileManagementService>();
builder.Services.AddScoped<SubjectsService>();
builder.Services.AddScoped<SubjectRepository>();
builder.Services.AddScoped<LevelService>();
builder.Services.AddScoped<LevelRepository>();
builder.Services.AddScoped<TrackExamService>();
builder.Services.AddScoped<TrackExamRepository>();
builder.Services.AddScoped<OrganizeExamRepository>();
builder.Services.AddScoped<OrganizeExamService>();
builder.Services.AddScoped<ProcessTakeExamService>();
builder.Services.AddScoped<ProcessTakeExamRepository>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<StatisticsRepository>();
builder.Services.AddSingleton<S3Service>();
builder.Services.AddScoped<IGenerateExamService, GenerateExamService>();
builder.Services.AddScoped<IGenerateExamRepository, GenerateExamRepository>();
builder.Services.AddScoped<ISubmitAnswerService, SubmitAnswerService>();
builder.Services.AddScoped<ISubmitAnswerRepository, SubmitAnswerRepository>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? string.Empty);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpsRedirection(o => o.HttpsPort = 5001);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Configuration.GetValue<bool>("EnableSwagger") || app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseRouting();

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { ok = true, env = app.Environment.EnvironmentName, time = DateTime.UtcNow }));
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
