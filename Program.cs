// See https://aka.ms/new-console-template for more information

using Octokit.GraphQL;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Model;
using Environment = System.Environment;

var productInformation = new ProductHeaderValue("Projectinator", "0.1.0");
var connection = new Connection(productInformation, Environment.GetEnvironmentVariable("GH_TOKEN"));

var username = "danielmarbach";
var ticks = DateTime.Now.Ticks.ToString();
var repoName = "TestRepo";

var repositoryQuery = new Query()
    .User(username)
    .Select(r => r.Id );

var userId = await connection.Run(repositoryQuery);

var projectName = $"ProjectName_{ticks}";
var clientMutationId = "abc123";

var createProject = new Mutation()
    .CreateProjectV2(new CreateProjectV2Input
    {
        OwnerId = userId,
        ClientMutationId = clientMutationId,
        Title = projectName,
    })
    .Select(payload => new
    {
        payload.ClientMutationId, ProjectId = payload.ProjectV2.Id, ProjectName = payload.ProjectV2.Title,
        ProjectOwnerId = payload.ProjectV2.Owner.Id, ProjectNumber = payload.ProjectV2.Number,
        //StatusField = payload.ProjectV2.Fields(20, null, null, null, null).Nodes.OfType<ProjectV2SingleSelectField>().Select(f => new { f.Id }).ToList().Single()
    });


var projectData =  await connection.Run(createProject);

var statusField = new Mutation()
    .CreateProjectV2Field(new CreateProjectV2FieldInput
    {
        ProjectId = projectData.ProjectId,
        DataType = ProjectV2CustomFieldType.SingleSelect,
        Name = "Status2",
        SingleSelectOptions = new ProjectV2SingleSelectFieldOptionInput[]
        {
            new() { Name = "To Do", Description = "First" },
            new() { Name = "To Do 1", Description = "Second" },
            new() { Name = "To Do 2", Description = "Third" },
        }
    })
    .Select(payload => new { Id = payload.ProjectV2Field });

var fieldData =  await connection.Run(statusField);

// var updateField = new Mutation()
//     .UpdateProjectV2ItemFieldValue(new UpdateProjectV2ItemFieldValueInput
//     {
//         ProjectId = projectData.ProjectId,
//         ClientMutationId = clientMutationId,
//         FieldId = projectData.StatusField.Id,
//         Value = new ProjectV2FieldValue
//         {
//             SingleSelectValue = new ProjectV2SingleSelectFieldValue
//             {
//                 Value = "In Progress"
//             }
//         }
//     })

var deleteProjectQuery = new Mutation()
    .DeleteProjectV2(new DeleteProjectV2Input
    {
        ProjectId = projectData.ProjectId,
        ClientMutationId = clientMutationId

    })
    .Select(payload => new
    {
        ProjectOwnerId = payload.ProjectV2.Owner.Id,
        payload.ClientMutationId,
    });

var deleteResult =  await connection.Run(deleteProjectQuery);