using System.Collections.Generic;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public interface IInspirationService
    {
        Task<IEnumerable<InspirationItem>> GetInspirationItemsAsync();
    }

    public class InspirationService : IInspirationService
    {
        public async Task<IEnumerable<InspirationItem>> GetInspirationItemsAsync()
        {
            // Mocking a "scrape" or API fetch
            await Task.Delay(1500); // Simulate network latency

            return new List<InspirationItem>
            {
                new InspirationItem 
                { 
                    Title = "Hollow Knight: Silksong", 
                    Author = "Team Cherry",
                    Description = "Explore a vast, haunted kingdom in Hollow Knight: Silksong! The sequel to the award winning action-adventure.",
                    Source = "Twitter",
                    ImageUrl = "https://assets1.ignimgs.com/2019/02/14/hollow-knight-silksong---button-1550186566835.jpg",
                    Likes = 15420,
                    Tags = "#2D #Metroidvania"
                },
                new InspirationItem 
                { 
                    Title = "Cities: Skylines II", 
                    Author = "Colossal Order",
                    Description = "Raise a city from the ground up and transform it into a thriving metropolis with the most realistic city builder ever.",
                    Source = "Unity Blog",
                    ImageUrl = "https://cdn.akamai.steamstatic.com/steam/apps/949230/capsule_616x353.jpg",
                    Likes = 8500,
                    Tags = "#Simulation #HDRP"
                },
                 new InspirationItem 
                { 
                    Title = "TUNIC", 
                    Author = "Andrew Shouldice",
                    Description = "Explore a land filled with lost legends, ancient powers, and ferocious monsters in TUNIC, an isometric action game about a small fox.",
                    Source = "Reddit",
                    ImageUrl = "https://image.api.playstation.com/vulcan/ap/rnd/202206/2719/Z4I7t6kZ4T2d3y4.png",
                    Likes = 12300,
                    Tags = "#Indie #Action"
                },
                new InspirationItem 
                { 
                    Title = "Genshin Impact", 
                    Author = "HoYoverse",
                    Description = "Step into Teyvat, a vast world teeming with life and flowing with elemental energy.",
                    Source = "Facebook",
                    ImageUrl = "https://cdn2.unrealengine.com/egs-genshin-impact-2-5-desktop-carousel-1248x702-602927233.jpg", // Actually Unity but just using a generic placeholder url if needed
                    Likes = 55000,
                    Tags = "#OpenWorld #Mobile"
                },
                new InspirationItem 
                { 
                    Title = "Cuphead", 
                    Author = "Studio MDHR",
                    Description = "Cuphead is a classic run and gun action game heavily focused on boss battles.",
                    Source = "Unity Case Studies",
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/en/2/2d/Cuphead_cover.png",
                    Likes = 32000,
                    Tags = "#2D #Animation"
                },
                new InspirationItem 
                { 
                    Title = "Among Us", 
                    Author = "Innersloth",
                    Description = "An online multiplayer social deduction game.",
                    Source = "Twitter",
                    ImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/945360/capsule_616x353.jpg",
                    Likes = 45000,
                    Tags = "#Multiplayer #2D"
                },
                new InspirationItem
                {
                    Title = "Sons of the Forest",
                    Author = "Endnight Games",
                    Description = "Sent to find a missing billionaire on a remote island, you find yourself in a cannibal-infested hellscape.",
                    Source = "Unity Forums",
                    ImageUrl = "https://cdn.akamai.steamstatic.com/steam/apps/1326470/capsule_616x353.jpg",
                    Likes = 9200,
                    Tags = "#Horror #Survival"
                },
                new InspirationItem
                {
                    Title = "Escape from Tarkov",
                    Author = "Battlestate Games",
                    Description = "A hardcore and realistic online first-person action RPG/Simulator with MMO features.",
                    Source = "Twitter",
                    ImageUrl = "https://cdn.akamai.steamstatic.com/steam/apps/1326470/header.jpg", // Placeholder
                    Likes = 22000,
                    Tags = "#FPS #Hardcore"
                }

            };
        }
    }
}
