using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using FireSharp;
using UniProjectCSharp.Models;
using Microsoft.AspNetCore.Mvc;

namespace UniProjectCSharp.Services
{
    public class GameService
    {
        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "GAvFJ1mhTpVhTSaXoURcUSACXWnCYkX1025wOaB5",
            BasePath = "https://uniwork-2df32-default-rtdb.europe-west1.firebasedatabase.app/",
        };

        private readonly IHttpContextAccessor _httpContextAccessor;
        IFirebaseClient client;

        public async Task<List<Games>> GetGames()
        {
            try
            {
                // Initialize Firebase client
                client = new FirebaseClient(ifc);

                if (client != null)
                {
                    Console.WriteLine("CLIENT WORKS FINE");
                }

                // Fetch data from the "games" node
                FirebaseResponse response = await client.GetAsync("games");

                Console.WriteLine($"Response: {response}");


                // Deserialize and return the data
                var data = response.ResultAs<List<Games>>();
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching games: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateGames(List<Games> newGames)
        {
            try
            {
                SetResponse response = client.Set("games", newGames);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateGames controller: {ex.Message}");
            }
        }

        public async Task<List<User>> Login(User data)
        {
            try
            {
                client = new FirebaseClient(ifc);

                if (client != null)
                {
                    Console.WriteLine("CLIENT WORKS FINE");
                }

                FirebaseResponse response = await client.GetAsync("users");

                var userData = response.ResultAs<List<User>>();

                return userData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("WE'RE IN ERROR FIELD");
                Console.WriteLine(ex.Message);
                return null;
            }
        }



        public async Task UpdateProfile(User newUser)
        {
            try
            {
                FirebaseResponse response = await client.GetAsync("users");
                var userData = response.ResultAs<List<User>>();

                int userIndex = userData.FindIndex(user => user.Email == newUser.Email);

                if (userIndex != -1)
                {
                    // Replace the old user with the new user
                    userData[userIndex] = newUser;

                    // Save the updated list back to Firebase
                    SetResponse updateResponse = await client.SetAsync("users", userData);
                }
                }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateGames UpdateProfile: {ex.Message}");
            }
        }

        public async Task CreateUser(User newUser)
        {
            FirebaseResponse response = await client.GetAsync("users");

            var userData = response.ResultAs<List<User>>();
            userData.Add(newUser);

            SetResponse pushResponse = await client.SetAsync("users", userData);
        }

    }
}
