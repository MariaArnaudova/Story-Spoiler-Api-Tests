using RestSharp;
using RestSharp.Authenticators;
using StorySpoilerApiTests.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoilerApiTests
{
    [TestFixture]
    public class StorySpoiler
    {
        private RestClient client;
        private static string createdStoryId;

        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string accessToken = GetJwtToken("MA1234", "MA1234");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(accessToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
                request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        [Order(1)]
        [Test]
        public void CreateANewStory()
        {
            var newStory = new
            {
                Title = "New story",
                Description = "New interesting and exiting story.",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
                request.AddJsonBody(newStory);

            var response = client.Execute(request);
            var jsonResponseContent = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Content, Does.Contain("storyId"));
            Assert.That(response.Content, Does.Contain("Successfully created!"), "Created story was unsuccessful.");

            createdStoryId = jsonResponseContent.GetProperty("storyId").GetString() ?? string.Empty;
        }

        [Order(2)]
        [Test]
        public void EditCreatedStorySpoiler()
        {
            var editedStorySpoiler = new
            {
                Title = "Edited story spoiler",
                Description = "Edited story spoiler interesting and exiting story.",
                Url = ""
            };

          //var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            var request = new RestRequest("/api/Story/Edit/{storyId}", Method.Put);
                request.AddUrlSegment("storyId", createdStoryId);
                request.AddJsonBody(editedStorySpoiler);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"), "Editing story was unsuccessful.");
        }

        [Order(3)]
        [Test]
        public void GetAllStorySpoilers()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = this.client.Execute(request);

            var responseAllStorySpoilers = JsonSerializer.Deserialize<List<StoryDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseAllStorySpoilers, Is.Not.Empty, "There is no story spoilers.");
        }

        [Order(4)]
        [Test]
        public void DeleteAStorySpoiler()
        {
            var request = new RestRequest("/api/Story/Delete/{storyId}", Method.Delete);
                request.AddUrlSegment("storyId", createdStoryId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"), "Deleting story was unsuccessful.");
        }

        [Order(5)]
        [Test]
        public void CreateAStorySpoilerWithoutRequiredFields()
        {
            var incompletedDataStory = new
            {
                Title = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
                request.AddJsonBody(incompletedDataStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistingStorySpoiler()
        {
            var nonExistingStoryId = "1243";

            var editedDataStorySpoiler = new
            {
                Title = "Edited title story spoiler",
                Description = "Edited description story spoiler story.",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Edit/{storyId}", Method.Put);
                request.AddUrlSegment("storyId", nonExistingStoryId);
                request.AddJsonBody(editedDataStorySpoiler);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."), "Non existing story.");
        }


        [Order(7)]
        [Test]
        public void DeleteANonExistingStorySpoiler()
        {
            var nonExistingStoryId = "1243";

            var request = new RestRequest("/api/Story/Delete/{storyId}", Method.Delete);
                request.AddUrlSegment("storyId", createdStoryId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"), "Non existing to delete this story.");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}