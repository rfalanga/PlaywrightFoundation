var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("postgres");
//.WithPgAdmin() // comment in if you want to explore the data with pgadmin
var crDb = db.AddDatabase("CarvedRockPostgres", "carved_rock");

var api = builder.AddProject<Projects.CarvedRock_Api>("carvedrock-api")
    .WithExternalHttpEndpoints()
    .WaitFor(crDb)
    .WithReference(crDb);

var email = builder.AddContainer("smptserver", "rnwood/smtp4dev")
    .WithHttpEndpoint(targetPort: 80, name: "http")
    .WithEndpoint(targetPort: 25, name: "smtp")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.CarvedRock_WebApp>("carvedrock-webapp")
    .WithEnvironment("CarvedRock__ApiBaseUrl", api.GetEndpoint("https"))
    .WithEnvironment("CarvedRock__SmtpServer", email.GetEndpoint("smtp"))
    .WithExternalHttpEndpoints();

builder.Build().Run();
