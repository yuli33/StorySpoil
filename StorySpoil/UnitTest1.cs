using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoil.Models;

namespace StorySpoil
{

    [TestFixture]
    public class StorySpoilTests
    {
        private RestClient client;
        private static string createdStoryId;
        private static string baseURL = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("guest1337", "Abcd1234");
            
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }
        
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseURL);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var story = new StoryDTO()
            {
                Title = "New Story",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            
            Assert.That(responseDto.StoryId, Is.Not.Null.Or.Empty);
            Assert.That(responseDto.Msg, Is.EqualTo("Successfully created!"));
            
            createdStoryId = responseDto.StoryId;
        }

        [Test, Order(2)]
        public void EditFoodTitle_ShouldReturnOk()
        {
            var updatedStory = new StoryDTO()
            {
                Title = "Updated Story",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseDto.Msg, Is.EqualTo("Successfully edited"));
        }
        
        [Test, Order(3)]
        public void GetAllStories_ShouldReturnList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var stories = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.That(stories, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseDto.Msg, Is.EqualTo("Deleted successfully!"));
        }
        
        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var story = new StoryDTO()
            {
                Title = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        
        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string fakeId = "non-existing-id";
            var updatedStory = new StoryDTO()
            {
                Title = "Updated Story",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(updatedStory);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            
            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseDto.Msg, Is.EqualTo("No spoilers..."));
        }
        
        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            string fakeId = "non-existing-id";
            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseDto.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }
        
        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}