using ContentHub.MockImplementation.Utils;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.Contracts.Notifications;
using Stylelabs.M.Sdk.Models.Notifications;
using Stylelabs.M.Sdk.WebClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;

namespace ContentHub.MockImplementation.Scripts
{
    class RejectionNotification
    {
        IWebMClient MClient = ContentHub.MockImplementation.Utils.MConnector.Client;
        const string emailTemplatename = "Asset Rejection Notification";
        public async Task SendEmail()
        {
            var rejectionTemplate = await MClient.Notifications.GetMailTemplateAsync(emailTemplatename);
            if (rejectionTemplate == null)
            {
                 CreateEmailTemaplate().Wait();
                 rejectionTemplate = await MClient.Notifications.GetMailTemplateAsync(emailTemplatename);
            }
            
            var group = MClient.Users.GetUserGroupAsync("Superusers", new EntityLoadConfiguration { RelationLoadOption= RelationLoadOption.All, PropertyLoadOption = PropertyLoadOption.All }).Result;
            var query = Query.CreateQuery(entities =>
                        from e in entities
                        where e.DefinitionName == "User"
                        select e);
            var users = MClient.Querying.QueryAsync(query, new EntityLoadConfiguration { RelationLoadOption = RelationLoadOption.All}).Result;

            var groupUsers = new List<IEntity>();
            foreach(var user in users.Items)
            {
                var groupRelation = user.GetRelation<IChildToManyParentsRelation>("UserGroupToUser");
                if (groupRelation.Parents.Contains(group.Id ?? 0)) // Superuser group Id
                    groupUsers.Add(user);
            }
            
            var userNames = MClient.Users.GetUsernamesAsync(groupUsers.Select(i => i.Id ?? 0).ToList()).Result?.Select(i => i.Value).ToList();
            
            //var user1 = MClient.Users.GetUserAsync(long.Parse(users.Items[3].Id.ToString()), new EntityLoadConfiguration { RelationLoadOption = RelationLoadOption.All, PropertyLoadOption = PropertyLoadOption.All }).Result;
            //var group1 = user1.Relations.FirstOrDefault(r => r.Name == "UserGroupToUser");
            var notificationRequest = new MailRequestByUsername
            {
                MailTemplateName = emailTemplatename,
                Recipients = userNames
            };

            notificationRequest.Variables.Add("AssetUrl", "https://mockimplementation.stylelabsdemo.com/en-us/asset/10481");
            await MClient.Notifications.SendEmailNotificationAsync(notificationRequest);
        }
        public async Task CreateEmailTemaplate()
        {
            CultureInfo enUs = CultureInfo.GetCultureInfo("en-US");
            IMailTemplate template = await MClient.EntityFactory.CreateAsync(Stylelabs.M.Sdk.Constants.MailTemplate.DefinitionName) as IMailTemplate;
            template.Name = emailTemplatename;
            template.SetSubject(enUs, "Asset Rejected");
            template.SetBody(enUs, "Hello, the following asset got rejected: {{AssetUrl}}");
            
            template.SetDescription(enUs, emailTemplatename);
            template.SetTemplateVariables(new[] {
                new TemplateVariable
                {
                    Name = "AssetUrl",
                    VariableType = TemplateVariableType.String
                }

            });

            await MClient.Entities.SaveAsync(template);
        }
    }
}
