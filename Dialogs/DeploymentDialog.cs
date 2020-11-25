//using AdaptiveCards;
using System.Web.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Attachment = Microsoft.Bot.Schema.Attachment;
using CoreBot.Bots;
using CoreBot.Helpers;

namespace CoreBot.Dialogs
{
    public class DeploymentDialog : CancelAndHelpDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;
        public DeploymentDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(DeploymentDialog))
        {
            Configuration = configuration;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new OptionPrompt(nameof(OptionPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                DestinationStepAsync,
                PortfolioStepAsync,
                DBdeploymentStepAsync,
                EnvironmentStepAsync,
                DBScriptCaptureStepAsync,
                DbInstanceStepAsync,
                VersionStepAsync,
                FileStepAsync,
                SprintNameStepAsync,
                DBRestrictedAsync,
                ConfirmStepAsync,
                CaptureEmailStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> DestinationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
             new PromptOptions
             {
                 Prompt = MessageFactory.Text("Please enter the portfolio"),
                 Choices = ChoiceFactory.ToChoices(new List<string> { "PCA", "CCV", "Rapid" }),
                 RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
             }, cancellationToken);
        }
        private async Task<DialogTurnResult> PortfolioStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            if (!string.IsNullOrEmpty(entitiDetails.Project))
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            entitiDetails.Portfolio = entitiDetails.Portfolio = ((FoundChoice)stepContext.Result).Value.ToString();
            if (entitiDetails.Portfolio.Equals("PCA"))
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
          new PromptOptions
          {
              Prompt = MessageFactory.Text("Please select the project"),
              Choices = GetProjectChoices(entitiDetails.Intent, entitiDetails.Role),
              Style = ListStyle.Auto,
              RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
          }, cancellationToken);
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("No deployments are enabled for selected portfolio. please try with other option "), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

        }
        private async Task<DialogTurnResult> DBdeploymentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            if (!string.IsNullOrEmpty(entitiDetails.Project) && !string.IsNullOrEmpty(entitiDetails.Environment))
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            else if (string.IsNullOrEmpty(entitiDetails.Project))
                entitiDetails.Project = ((FoundChoice)stepContext.Result).Value.ToString();
            if (entitiDetails.Project == "DB-Deployment")
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select DB-Deployment type"),
                    Choices = GetProjectChoices(entitiDetails.Project, entitiDetails.Role),
                    RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                }, cancellationToken);
            else if (entitiDetails.Project == "DB-Operations")
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select DB-Operation type"),
                    Choices = GetProjectChoices(entitiDetails.Project, entitiDetails.Role),
                    RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                }, cancellationToken);
            else if (entitiDetails.Project == "Build-Artifact")
            {
                List<string> apps = GetCIApplications().Keys.ToList();
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select the application"),
                    Choices = ChoiceFactory.ToChoices(apps),// GetProjectChoices(entitiDetails.Project, entitiDetails.Role),
                    RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                }, cancellationToken);
            }
            else if (entitiDetails.Project == "ICMS-Realtime-Fuse")
            {
                List<string> apps = GetICMSClientNames().Keys.ToList();
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select the Client Name"),
                    Choices = ChoiceFactory.ToChoices(apps),// GetProjectChoices(entitiDetails.Project, entitiDetails.Role),
                    Style = ListStyle.List,
                    RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                }, cancellationToken);
            }
            else if (entitiDetails.Project == "ICM-Jar-Deploy")
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Please select the build number to deploy"),
                     Choices = GetSuccessfulBuildNumbers(),
                     Style = ListStyle.List,
                     RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                 }, cancellationToken);
            }
            else
                return await stepContext.NextAsync(entitiDetails, cancellationToken);

        }
        private async Task<DialogTurnResult> EnvironmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            if (!string.IsNullOrEmpty(entitiDetails.Project) && !string.IsNullOrEmpty(entitiDetails.Environment))
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            else if (entitiDetails.Project == "DB-Deployment")
            {
                entitiDetails.ScriptName = ((FoundChoice)stepContext.Result).Value.ToString();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("PM.SQL", "PCA_Sql_Runner");
                dic.Add("API.SQL", "API_PCA_Sql_Runner");
                dic.Add("ICMSDB.SQL", "ICMS_DB_Deployer");
                dic.Add("Custom", "API_PCA_Sql_Runner");
                dic.Add("Schema Wise", "API_PCA_Sql_Runner");
                entitiDetails.DBDeploymenttype = dic[entitiDetails.ScriptName];
                if (entitiDetails.ScriptName == "Custom" || entitiDetails.ScriptName == "Schema Wise")
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter the script name to deploy") }, cancellationToken);
            }
            else if (entitiDetails.Project == "DB-Operations")
            {
                entitiDetails.ScriptName = ((FoundChoice)stepContext.Result).Value.ToString();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("LotusNotes", "LEI_Start_Stop");
                dic.Add("delphix", "dx_vdb_operations");
                entitiDetails.DBDeploymenttype = dic[entitiDetails.ScriptName];
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("Please select DB-Operation type"),
                   Choices = GetProjectChoices(entitiDetails.ScriptName, entitiDetails.Role),
                   RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
               }, cancellationToken);
            }
            else if (entitiDetails.Project == "Build-Artifact")
            {
                entitiDetails.ScriptName = ((FoundChoice)stepContext.Result).Value.ToString();
                entitiDetails.DBDeploymenttype = GetCIApplications()[entitiDetails.ScriptName];

                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else if (entitiDetails.Project == "ICMS-Realtime-Fuse")
            {
                entitiDetails.Client = ((FoundChoice)stepContext.Result).Value.ToString();
                entitiDetails.HostName = GetICMSClientNames()[entitiDetails.Client];

                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Please select the version to deploy"),
                     Choices = GetNexusVersions(),
                     Style = ListStyle.Auto,
                     RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                 }, cancellationToken);
            }
            else if (entitiDetails.Project == "ICM-Jar-Deploy")
            {
                entitiDetails.BuildNumber = ((FoundChoice)stepContext.Result).Value.ToString();
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select the host name"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "usadcqdom05" }),
                    RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                }, cancellationToken);
            }
            return await stepContext.NextAsync(entitiDetails, cancellationToken);

        }

        private async Task<DialogTurnResult> DBScriptCaptureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            if (!string.IsNullOrEmpty(entitiDetails.Project) && !string.IsNullOrEmpty(entitiDetails.Environment))
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            else if (entitiDetails.Project == "DB-Deployment" && (entitiDetails.ScriptName == "Custom" || entitiDetails.ScriptName == "Schema Wise"))
                entitiDetails.ScriptName = entitiDetails.ScriptName == "Schema Wise" ? "Schema Wise/" + stepContext.Result.ToString() : stepContext.Result.ToString();
            else if (entitiDetails.Project == "DB-Operations")
            {
                entitiDetails.ScheduledOption = ((FoundChoice)stepContext.Result).Value.ToString();
                if (entitiDetails.ScriptName == "LotusNotes")
                    return await stepContext.PromptAsync(nameof(ChoicePrompt),
        new PromptOptions
        {
            Prompt = MessageFactory.Text("Please select the Environment"),
            Choices = ChoiceFactory.ToChoices(GetEnvironments(entitiDetails.Project, string.IsNullOrEmpty(entitiDetails.ScriptName) ? string.Empty : entitiDetails.ScriptName)),
            RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
        }, cancellationToken);
                else
                    return await stepContext.PromptAsync(nameof(ChoicePrompt),
       new PromptOptions
       {
           Prompt = MessageFactory.Text("Please select the VDB"),
           Choices = ChoiceFactory.ToChoices(GetEnvironments("VDB", string.IsNullOrEmpty(entitiDetails.ScriptName) ? string.Empty : entitiDetails.ScriptName)),
           RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
       }, cancellationToken);
            }
            else if (entitiDetails.Project == "ICM-Jar-Deploy")
            {
                entitiDetails.HostName = ((FoundChoice)stepContext.Result).Value.ToString();
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select the Environment"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "VPMSPTE" }),
                    RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                }, cancellationToken);
            }
            else if (entitiDetails.Project == "ICMS-Realtime-Fuse")
            {
                entitiDetails.Buildversion = ((FoundChoice)stepContext.Result).Value.ToString();
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
      new PromptOptions
      {
          Prompt = MessageFactory.Text("Please select the Environment"),
          Choices = ChoiceFactory.ToChoices(new List<string> { "QA" }),
          RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
      }, cancellationToken);
            }

            return await stepContext.PromptAsync(nameof(ChoicePrompt),

    new PromptOptions
    {
        Prompt = MessageFactory.Text("Please select the environment"),
        Choices = ChoiceFactory.ToChoices(GetEnvironments(entitiDetails.Project, string.IsNullOrEmpty(entitiDetails.ScriptName) ? string.Empty : entitiDetails.ScriptName)),
        RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
    }, cancellationToken);


        }
        private async Task<DialogTurnResult> DbInstanceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            if (entitiDetails.Environment == null && entitiDetails.Project != "DB-Operations")
                entitiDetails.Environment = ((FoundChoice)stepContext.Result).Value.ToString();
            if (entitiDetails.Project == "App-Deployment")
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter war to deploy the build.\n\n" + " Ex:Ipp-Portal:<version>,Loginservice:<version>,Client-Profile:<version>") }, cancellationToken);
            else if (entitiDetails.Project == "CIT-Deployment")
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter war to deploy the build.\n\n" + " Ex: hello-world.war:<version>") }, cancellationToken);
            else if (entitiDetails.Project == "DB-Operations")
            {
                entitiDetails.Environment = ((FoundChoice)stepContext.Result).Value.ToString();
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else if (entitiDetails.Project == "RMI-Deployment" || entitiDetails.Project == "DB-Deployment")
            {
                if (entitiDetails.Project == "RMI-Deployment")
                    entitiDetails.HostName = entitiDetails.Environment == "QA(VPMTST1)" ? "usdtrmi03" : "usddevrmi01";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("QA(VPMTST1)", "VPMTST1");
                dic.Add("SprintTest(VPMSPTE)", "VPMSPTE");
                dic.Add("SprintDemo(VPMDEMO)", "VPMDEMO");
                dic.Add("CICD(VPMCICD)", "VPMCICD");
                dic.Add("CVQA(VCSQPCV5)", "VCSQPCV5");
                dic.Add("CVDEV(VCSPCV5)", "VCSPCV5");
                dic.Add("CVE2E(CSPERF10)", "CSPERF10");
                entitiDetails.DbInstance = dic[entitiDetails.Environment].ToString();
                if (entitiDetails.Project == "DB-Deployment")
                    return await stepContext.PromptAsync(nameof(ChoicePrompt),
          new PromptOptions
          {
              Prompt = MessageFactory.Text("Please select from where you wanto to deploy?"),
              Choices = ChoiceFactory.ToChoices(new List<string> { "trunk", "tags" }),
              Style = ListStyle.Auto,
              RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
          }, cancellationToken);
                else
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter the repository for deployment") }, cancellationToken);
            }
            else if (entitiDetails.Project == "Informatica-Deployment")
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
          new PromptOptions
          {
              Prompt = MessageFactory.Text("Please select the repository branch or press 1 to skip the branch selection"),
              Choices = GetBranches(entitiDetails.Environment),
              Style = ListStyle.Auto,
              RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
          }, cancellationToken);
            else if (entitiDetails.Project == "Build-Artifact")
            {
                if (!entitiDetails.ScriptName.Equals("Client Inquiry Snapshot"))
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter the branch name to build artifact") }, cancellationToken);
                else
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }

            else if (entitiDetails.Project == "ICMS-Realtime-Fuse")
            {
                entitiDetails.Environment = ((FoundChoice)stepContext.Result).Value.ToString();
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Do you want to do a force deployment?") }, cancellationToken);
            }
            else if (entitiDetails.Project == "ICM-Jar-Deploy")
            {
                entitiDetails.Environment = ((FoundChoice)stepContext.Result).Value.ToString();
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter the version to be deployed") }, cancellationToken);
            }
            else
            {
                if (entitiDetails.Buildwar == null && entitiDetails.Project != "ClientInquiry-Deployment")
                    entitiDetails.Buildwar = (string)stepContext.Result;
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> VersionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            switch (entitiDetails.Project)
            {
                case "Informatica-Deployment":
                    entitiDetails.Repo = ((FoundChoice)stepContext.Result).Value.ToString();
                    entitiDetails.Repo = entitiDetails.Repo.Equals("Skip") ? string.Empty : entitiDetails.Repo;
                    string verparam = entitiDetails.Environment;
                    if (!entitiDetails.Environment.Equals("SBX"))
                        verparam = GetEnvironments(entitiDetails.Project, "")[GetEnvironments(entitiDetails.Project, "").IndexOf(entitiDetails.Environment) - 1];
                    else
                        return await stepContext.NextAsync(entitiDetails, cancellationToken);
                    IList<string> etlVersions = OracleHelper.getVersionsfromInfluxDb(Configuration, verparam);
                    if (etlVersions != null)
                        return await stepContext.PromptAsync(nameof(ChoicePrompt),
                  new PromptOptions
                  {
                      Prompt = MessageFactory.Text("Please select the version of a branch need to deploy"),
                      Choices = ChoiceFactory.ToChoices(etlVersions),
                      Style = ListStyle.Auto,
                      RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                  }, cancellationToken);
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please deploy the version in lower environment."), cancellationToken);
                        return await stepContext.EndDialogAsync(null, cancellationToken);

                    }
                case "App-Deployment":
                    entitiDetails.Buildwar = (string)stepContext.Result;
                    var msg = $"Please confirm, Do you want to Deploy build for the war {entitiDetails.Buildwar} to {entitiDetails.Environment} environment ?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Do you want to do a force deployment?") }, cancellationToken);
                case "DB-Deployment":
                    entitiDetails.Repo = ((FoundChoice)stepContext.Result).Value.ToString();
                    return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please select the repository name deployment"),
                        Choices = ChoiceFactory.ToChoices(OracleHelper.getOracleDBBranches(Configuration)),
                        Style = ListStyle.Auto,
                        RetryPrompt = MessageFactory.Text("Sorry, I'm still learning. Please provide the valid option or below mentioned Sequence Number."),
                    }, cancellationToken);
                case "CIT-Deployment":
                    entitiDetails.Buildwar = (string)stepContext.Result;
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);
                case "RMI-Deployment":
                    entitiDetails.Repo = (string)stepContext.Result;
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);
                case "Build-Artifact":
                    if (!entitiDetails.ScriptName.Equals("Client Inquiry Snapshot"))
                        entitiDetails.Buildversion = stepContext.Result.ToString();
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);
                case "ClientInquiry-Deployment":
                    entitiDetails.DBDeploymenttype = "PCA_Client-Inquiry/job/PCA_Client-Inquiry_Deploy";
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter the version to be deployed") }, cancellationToken);
                case "ICMS-Realtime-Fuse":
                    entitiDetails.isForceDeployment = (bool)stepContext.Result;
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);
                case "ICM-Jar-Deploy":
                    entitiDetails.Buildversion = (string)stepContext.Result;
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);
                // return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Do you want to deploy through Floraa?") }, cancellationToken);
                default:
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);

            }
        }
        private async Task<DialogTurnResult> FileStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            if (entitiDetails.Project == "Informatica-Deployment")
            {
                if (!entitiDetails.Environment.Equals("SBX"))
                {
                    entitiDetails.Buildversion = ((FoundChoice)stepContext.Result).Value.ToString();
                    entitiDetails.Buildversion = entitiDetails.Buildversion.Equals("Skip") ? string.Empty : entitiDetails.Buildversion;
                }
                else entitiDetails.Buildversion = string.Empty;
                if (!string.IsNullOrEmpty(entitiDetails.Repo))
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter file name to deploy the build. Type N/A if not appicable\n\n" + " Ex:LV_NCD_NON_LAB.zip") }, cancellationToken);
                else
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else if (entitiDetails.Project == "App-Deployment")
            {
                entitiDetails.isForceDeployment = (bool)stepContext.Result;
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else if (entitiDetails.Project == "DB-Deployment")
            {
                if (!entitiDetails.Repo.Equals("trunk"))
                    entitiDetails.Repo = ((FoundChoice)stepContext.Result).Value.ToString();
                entitiDetails.Buildwar = ((FoundChoice)stepContext.Result).Value.ToString();
                if (entitiDetails.ScriptName.Equals("PM.SQL") || entitiDetails.ScriptName.Equals("ICMSDB.SQL"))
                {
                    string strMsg = "Please confirm, Do you want to do code cutoff?";
                    strMsg = entitiDetails.ScriptName.Equals("ICMSDB.SQL") ? "Please confirm, Do you want to take a snapshot before deployment?" : strMsg;
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(strMsg) }, cancellationToken);
                }
                else
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else if (entitiDetails.Project == "ClientInquiry-Deployment")
            {
                entitiDetails.Buildversion = stepContext.Result.ToString();
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter F5 URL or Node URL to deploy") }, cancellationToken);
            }
            else
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
        }
        private async Task<DialogTurnResult> SprintNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            if (entitiDetails.Project == "DB-Deployment")
            {
                if (entitiDetails.ScriptName.Equals("PM.SQL") || entitiDetails.ScriptName.Equals("ICMSDB.SQL"))
                {
                    entitiDetails.codeCutOff = (bool)stepContext.Result;
                    if (entitiDetails.codeCutOff && entitiDetails.ScriptName.Equals("PM.SQL"))
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter sprint name for code cut off or type NA if not") }, cancellationToken);
                    else
                        return await stepContext.NextAsync(entitiDetails, cancellationToken);
                }
                else return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else if (entitiDetails.Project == "Informatica-Deployment")
            {
                if (string.IsNullOrEmpty(entitiDetails.File))
                    entitiDetails.File = (stepContext.Result.ToString().Trim().ToUpper().Equals("N/A") || stepContext.Result.ToString().Trim().ToUpper().Equals("NA") || string.IsNullOrEmpty(entitiDetails.Repo)) ? string.Empty : stepContext.Result.ToString();
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else if (entitiDetails.Project == "ClientInquiry-Deployment")
            {
                entitiDetails.HostName = stepContext.Result.ToString();
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
        }
        private async Task<DialogTurnResult> DBRestrictedAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            if (entitiDetails.Project == "DB-Deployment")
            {
                if (entitiDetails.ScriptName.Equals("PM.SQL"))
                {
                    entitiDetails.SprintName = stepContext.Result.ToString().Trim().ToUpper().Equals("N/A") || stepContext.Result.ToString().Trim().ToUpper().Equals("NA") ? string.Empty : stepContext.Result.ToString();
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please confirm, Do you want to proceed DB deployment in restricted mode?") }, cancellationToken);
                }
                else return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else if (entitiDetails.Project == "Informatica-Deployment")
            {
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
            else
                return await stepContext.NextAsync(entitiDetails, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            string msg = string.Empty;
            switch (entitiDetails.Project)
            {
                case "Informatica-Deployment":
                    msg = $"Please confirm, Do you want to proceed with ETL deployment on {entitiDetails.Environment} environment for {entitiDetails.Repo} repository?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
                case "App-Deployment":
                case "CIT-Deployment":
                    if (entitiDetails.Buildwar == null)
                        entitiDetails.Buildwar = (string)stepContext.Result;
                    msg = $"Please confirm, Do you want to Deploy build for the war {entitiDetails.Buildwar} to {entitiDetails.Environment} environment ?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
                case "DB-Deployment":
                    if (entitiDetails.ScriptName.Equals("PM.SQL"))
                        entitiDetails.isDBRestricted = (bool)stepContext.Result;
                    msg = $"Please confirm, Do you want to proceed with {entitiDetails.ScriptName} deployment from {entitiDetails.Repo} repository with '{entitiDetails.Buildwar}' on {entitiDetails.DbInstance}?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
                case "RMI-Deployment":
                    if (string.IsNullOrEmpty(entitiDetails.Repo))
                        entitiDetails.Repo = (string)stepContext.Result;
                    msg = $"Please confirm, Do you want to proceed with RMI deployment on {entitiDetails.Environment} environment for {entitiDetails.Repo} repository?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
                case "DB-Operations":
                    msg = $"Please confirm, Do you want to perform {entitiDetails.ScheduledOption} operation on {entitiDetails.ScriptName} ?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
                case "ClientInquiry-Deployment":
                    msg = $"Please confirm, Do you want to proceed with Client Inquiry deployment in {entitiDetails.Environment} with version {entitiDetails.Buildversion} ?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
                case "Build-Artifact":
                    msg = $"Please confirm, Do you want to build {entitiDetails.ScriptName} for {entitiDetails.Environment} environment?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
                case "ICMS-Realtime-Fuse":
                    msg = $"Please confirm, Do you want {entitiDetails.Project} deployement for {entitiDetails.Client} client with {entitiDetails.Buildversion} version in {entitiDetails.Environment} environment?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
                case "ICM-Jar-Deploy":
                    msg = $"Please confirm, Do you want {entitiDetails.Project} for {entitiDetails.BuildNumber} build number {entitiDetails.Buildversion} version in {entitiDetails.Environment} environment?";
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
                default:
                    return await stepContext.NextAsync(entitiDetails, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> CaptureEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {  //var bookingDetails = await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken);
            var entitiDetails = (EntitiDetails)stepContext.Options;
            if (stepContext.Index > 11 || (bool)stepContext.Result)
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Enter your cotiviti email id to receive the status") }, cancellationToken);
            else
                return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var entitiDetails = (EntitiDetails)stepContext.Options;
            entitiDetails.Email = (string)stepContext.Result;
            if (!(entitiDetails.Email.ToLower().Contains("@cotiviti.com")))
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                return await CaptureEmailStepAsync(stepContext, cancellationToken);
            }
            return await stepContext.EndDialogAsync(entitiDetails, cancellationToken);


        }
        private IList<Choice> GetProjectChoices(string sProject, string sRole)
        {
            switch (sProject.ToUpper())
            {

                case "BUILD_DEPLOYMENT":
                    var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "App-Deployment", Synonyms = new List<string>() { "App" } },
                new Choice() { Value = "Informatica-Deployment", Synonyms = new List<string>() { "ETL" } },
                new Choice() { Value = "RMI-Deployment", Synonyms = new List<string>() { "RMI" } },
                new Choice() { Value = "CIT-Deployment", Synonyms = new List<string>() { "CIT","WIT" } },
                new Choice() { Value = "DB-Deployment",  Synonyms = new List<string>() { "DB" } },
                new Choice() { Value = "DB-Operations",  Synonyms = new List<string>() { "DB Operations" } },
                new Choice() { Value = "ClientInquiry-Deployment",  Synonyms = new List<string>() { "Client Inquiry" } },
                new Choice() { Value = "Build-Artifact",  Synonyms = new List<string>() { "Build Artifact" } },
                new Choice() { Value = "ICMS-Realtime-Fuse",  Synonyms = new List<string>() { "ICMS-Realtime-Fuse" } },
                new Choice() { Value = "ICM-Jar-Deploy",  Synonyms = new List<string>() { "ICM-Jar-Deploy" } },
            };
                    return cardOptions;
                case "DB-DEPLOYMENT":
                    MainDialog mainDialog = new MainDialog(Configuration, null, null);
                    string[] actions = mainDialog.getActions(sRole, "SubAction");
                    cardOptions = new List<Choice>();
                    foreach (string action in actions)
                        cardOptions.Add(new Choice() { Value = action, Synonyms = new List<string>() { action } });

                    return cardOptions;
                case "DB-OPERATIONS":
                    cardOptions = new List<Choice>()
            {
                new Choice() { Value = "LotusNotes", Synonyms = new List<string>() { "LotusNotes" } },
                new Choice() { Value = "delphix", Synonyms = new List<string>() { "LotusNotes" } },
            };
                    return cardOptions;
                case "LOTUSNOTES":
                    cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Start", Synonyms = new List<string>() { "Start" } },
                new Choice() { Value = "Stop", Synonyms = new List<string>() { "Stop" } },
            };
                    return cardOptions;
                case "DELPHIX":
                    cardOptions = new List<Choice>()
            {
                new Choice() { Value = "snapshot", Synonyms = new List<string>() { "snapshot" } },
                new Choice() { Value = "rewind", Synonyms = new List<string>() { "rewind" } },
            };
                    return cardOptions;
                case "BUILD-ARTIFACT":
                    cardOptions = new List<Choice>() {
                new Choice() { Value = "PCA Rules Catalog", Synonyms = new List<string>() { "Rules catalog" } },
                new Choice() { Value = "PCA Security Manager", Synonyms = new List<string>() { "Security manager" } },
                new Choice() { Value = "Client Inquiry Snapshot", Synonyms = new List<string>() { "snapshot" } },
                    };
                    return cardOptions;
                default:
                    return null;
            }
        }

        private IList<string> GetEnvironments(string strType, string dbScript)
        {
            switch (strType)
            {
                case "App-Deployment":
                    var cardOptions = new List<string>() { "QA", "SPTE", "DEMO", "CICD" };

                    return cardOptions;
                case "Informatica-Deployment":
                    cardOptions = new List<string>() { "SBX", "SDEV", "SQA", "QA", "SUAT", "UAT", };

                    return cardOptions;
                case "RMI-Deployment":
                case "DB-Deployment":
                    cardOptions = new List<string>() { "SprintTest(VPMSPTE)", "SprintDemo(VPMDEMO)", "CICD(VPMCICD)" };

                    if (!dbScript.Equals("LotusNotes"))
                        cardOptions.Add("QA(VPMTST1)");

                    if (dbScript.Equals("ICMSDB.SQL"))
                    {
                        cardOptions.Add("CVQA(VCSQPCV5)");
                        cardOptions.Add("CVDEV(VCSPCV5)");
                        cardOptions.Add("CVE2E(CSPERF10)");
                    }
                    return cardOptions;
                case "CIT-Deployment":
                    cardOptions = new List<string>() { "DEV", "QA" };

                    return cardOptions;
                case "ClientInquiry-Deployment":
                case "Build-Artifact":

                    cardOptions = new List<string>() { "DEV", "QA", "UAT", "PRE-PROD" };
                    if (dbScript.Equals("PCA Rules Catalog") || dbScript.Equals("PCA Security Manager"))
                        cardOptions = new List<string>() { "LOCAL", "DEV", "QA", "UAT", "PRE-PROD" };
                    return cardOptions;
                case "DB-Operations":
                    cardOptions = new List<string>() { "SPTE", "DEMO", "CICD" };

                    return cardOptions;
                case "VDB":
                    cardOptions = new List<string>() { "VPMCICD", "VPMSPTE", "VPMDEMO", "VCSQPCV5", "VCSPCV5", "VPMJKPOC" };

                    return cardOptions;
                default:
                    return null;
            }
        }

        private HttpClient getHttpClient(string url, bool isAuthreq)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(url);
            if (isAuthreq)
                client.DefaultRequestHeaders.Add("Authorization", Configuration["BitbuketAuthKey"]);
            return client;

        }
        private IList<Choice> GetBranches(string env)
        {

            var url = Configuration["ETLBitbucketUrl"];
            var client = getHttpClient(Configuration["ETLBitbucketUrl"], true);
            HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var res = (JObject)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            var values = res["values"].Children();
            var cardOptions = new List<Choice>();
            if (!env.Equals("SBX"))
                cardOptions.Add(new Choice() { Value = "Skip" });
            foreach (var value in values)
                cardOptions.Add(new Choice() { Value = value["displayId"].ToString() });
            return cardOptions;

        }
        private Dictionary<string, string> GetCIApplications()
        {
            Dictionary<string, string> dicCIApps = new Dictionary<string, string>();
            dicCIApps.Add("PCA Rules Catalog", "PCA_CIT-WIT_CI_JOBS/job/PCA_CIT-WIT-RulesCatalog_client");
            dicCIApps.Add("PCA Security Manager", "PCA_CIT-WIT_CI_JOBS/job/PCA_CIT-WIT-pca-securitymanager_client");
            dicCIApps.Add("Client Inquiry Snapshot", "PCA_Client-Inquiry/job/PCA_Client-Inquiry_Snapshot");

            return dicCIApps;
        }
        private Dictionary<string, string> GetICMSClientNames()
        {
            Dictionary<string, string> dicCIApps = new Dictionary<string, string>();
            dicCIApps.Add("AETAS", "icms-realtime-aet.war");
            dicCIApps.Add("AMGFA", "icms-realtime-amg.war");
            dicCIApps.Add("ANACE", "icms-realtime-anace.war");

            dicCIApps.Add("ANCS9", "icms-realtime-ancs9.war");
            dicCIApps.Add("ANLCF", "icms-realtime-anlcf.war");
            dicCIApps.Add("ANNAS", "icms-realtime-ann.war");
            dicCIApps.Add("ANWGS", "icms-realtime-anw.war");
            dicCIApps.Add("BCSMC", "icms-realtime-bcs.war");
            dicCIApps.Add("BCTFP", "icms-realtime-bct.war");

            dicCIApps.Add("HARMA", "icms-realtime-har.war");
            dicCIApps.Add("HUEDG", "icms-realtime-huedg.war");
            dicCIApps.Add("HZNHP", "icms-realtime-hzn.war");
            dicCIApps.Add("TMHTX", "icms-realtime-tmhtx.war");
            dicCIApps.Add("WELFE", "icms-realtime-welfe.war");
            dicCIApps.Add("WPTWG", "icms-realtime-wptwg.war");


            return dicCIApps;
        }


        private IList<Choice> GetNexusVersions()
        {
            try
            {
                var xmlFile = Configuration["ICMSNexusURL"];
                var client = getHttpClient(Configuration["ICMSNexusURL"], false);
                HttpResponseMessage response = client.GetAsync(xmlFile).GetAwaiter().GetResult();
                XDocument xmlDoc = XDocument.Parse(response.Content.ReadAsStringAsync().Result);
                var xmlNodes = xmlDoc.Descendants("version");
                var cardOptions = new List<Choice>();
                foreach (var item in xmlNodes)
                    cardOptions.Add(new Choice() { Value = item.Value });
                return cardOptions;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /*private IList<Choice> GetSuccessfulBuildNumbers()
        {
            try
            {
                var xmlFile = Configuration["ICMSBuildURL"];
                var client = getHttpClient(Configuration["ICMSBuildURL"], false);
                HttpResponseMessage response = client.GetAsync(xmlFile).GetAwaiter().GetResult();
                XDocument xmlDoc = XDocument.Parse(response.Content.ReadAsStringAsync().Result);
                var xmlNodes = xmlDoc.Descendants("number");
                var cardOptions = new List<Choice>();
                foreach (var item in xmlNodes)
                    cardOptions.Add(new Choice() { Value = item.Value });
                return cardOptions;
            }
            catch (Exception ex)
            {

                throw;
            }

        }*/

        private IList<Choice> GetSuccessfulBuildNumbers()
        {
            try
            {

                var client = getHttpClient(Configuration["ICMSBuildURL"], false);
                HttpResponseMessage response = client.GetAsync(Configuration["ICMSBuildURL"]).GetAwaiter().GetResult();
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result).Values().Children();
                var cardOptions = new List<Choice>();
                foreach (var r in root)
                    if (r["result"].ToString() == "SUCCESS")
                        cardOptions.Add(new Choice() { Value = r["number"].ToString() });
                return cardOptions;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
