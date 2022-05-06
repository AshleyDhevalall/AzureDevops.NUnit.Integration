using System;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevops.Service
{
    public class AzureDevopsApi
    {
        private TestManagementHttpClient _testManagementClient;

        public AzureDevopsApi(string azureDevopsUrl, string pat)
        {
            var credentials = new PatCredentials("", pat);
            var connection = new VssConnection(new Uri(azureDevopsUrl), credentials);
            _testManagementClient = connection.GetClient<TestManagementHttpClient>();
        }

        public void PrintBasicRunInfo(TestRun testRun)
        {
            Console.WriteLine("Information for test run:" + testRun.Id);
            Console.WriteLine("Automated - {0}; Start Date - '{1}'; Completed date - '{2}'", (testRun.IsAutomated) ? "Yes" : "No", testRun.StartedDate.ToString(), testRun.CompletedDate.ToString());
            Console.WriteLine("Total tests - {0}; Passed tests - {1}", testRun.TotalTests, testRun.PassedTests);
        }

        public TestRun CreateTestRunAsync(string TeamProjectName)
        {
            RunCreateModel runCreate = new RunCreateModel(
                      name: "Test run from console - completed",
                      startedDate: DateTime.Now.ToString("o"),
                      isAutomated: true
                   );

            TestRun testRun = _testManagementClient.CreateTestRunAsync(runCreate, TeamProjectName).Result;
            return testRun;
        }

        public void AddResultsForCases(int runId, string TeamProjectName, System.Collections.Generic.List<TestCaseResult> testCaseResults)
        {
            _testManagementClient.AddTestResultsToTestRunAsync(testCaseResults.ToArray(), TeamProjectName, runId).Wait();

            RunUpdateModel runUpdateModel = new RunUpdateModel(
                completedDate: DateTime.Now.ToString("o"),
                state: Enum.GetName(typeof(TestRunState), TestRunState.Completed)
                );

            var testRun = _testManagementClient.UpdateTestRunAsync(runUpdateModel, TeamProjectName, runId).Result;

            PrintBasicRunInfo(testRun);
        }
    }
}
