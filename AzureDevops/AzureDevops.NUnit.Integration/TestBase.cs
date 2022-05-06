using AzureDevops.Service;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;

namespace AzureDevops.NUnit.Integration
{
    public class TestBase
    {
        private TestContext _fixtureContext;
        private AzureDevopsApi _azuredevopsApi;
        private bool IgnoreAddResults = false;
        private string _suiteid, _projectid;
        //private int _projectIdInt, _suiteIdInt, _caseId;
        private System.Collections.Generic.List<TestCaseResult> _resultsForCases;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            try
            {
                _fixtureContext = TestContext.CurrentContext;
                InitAzureDevopsConfig();
                ValidateSuiteIdAndProjectId();
                _resultsForCases = new System.Collections.Generic.List<TestCaseResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                IgnoreAddResults = true;
            }
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            if (!IgnoreAddResults)
            {
                if (_resultsForCases.Count > 0)
                {
                    var testRun = _azuredevopsApi.CreateTestRunAsync("SampleProject");
                    if (testRun.Id > 0) _azuredevopsApi.AddResultsForCases(testRun.Id, "SampleProject", _resultsForCases);
                }
            }
        }

        [TearDown]
        public void Cleanup()
        {
            if (!IgnoreAddResults)
            {
                TestCaseResult testCaseResult = new TestCaseResult();
                testCaseResult.AutomatedTestName = $"{TestContext.CurrentContext.Test.ClassName}.{TestContext.CurrentContext.Test.MethodName}";
                testCaseResult.TestCaseTitle = TestContext.CurrentContext.Test.Name;
                testCaseResult.CompletedDate = DateTime.Now;

                var resultState = TestContext.CurrentContext.Result.Outcome;
                if (resultState == ResultState.Success) testCaseResult.Outcome = Enum.GetName(typeof(TestOutcome), TestOutcome.Passed);
                else if (resultState == ResultState.Inconclusive) testCaseResult.Outcome = Enum.GetName(typeof(TestOutcome), TestOutcome.Inconclusive);
                else testCaseResult.Outcome = Enum.GetName(typeof(TestOutcome), TestOutcome.Failed);

                testCaseResult.State = Enum.GetName(typeof(TestRunState), TestRunState.Completed);
                _resultsForCases.Add(testCaseResult);
            }
        }

        private void InitAzureDevopsConfig()
        {
            string azuredevopsurl = ConfigurationManager.AppSetting["AzureDevopsUrl"];
            string pat = ConfigurationManager.AppSetting["PAT"];

            if (string.IsNullOrEmpty(azuredevopsurl)) throw new Exception("Invalid azure devops url");
            if (string.IsNullOrEmpty(pat)) throw new Exception("Invalid azure devops pat token");

            _azuredevopsApi = new AzureDevopsApi(azuredevopsurl, pat);
        }

        private void ValidateSuiteIdAndProjectId()
        {
            _suiteid = _fixtureContext.Test.Properties.Get("suiteid")?.ToString();
            _projectid = _fixtureContext.Test.Properties.Get("projectid")?.ToString();

            if (string.IsNullOrEmpty(_suiteid)) throw new Exception("Invalid suite id");
            if (string.IsNullOrEmpty(_projectid)) throw new Exception("Invalid project id");
        }
    }
}
