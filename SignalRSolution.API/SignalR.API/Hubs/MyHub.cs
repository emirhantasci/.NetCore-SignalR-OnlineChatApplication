using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalR.API.Models;

namespace SignalR.API.Hubs
{
    public class MyHub:Hub
    {

        private static List<string> Names { get; set; } = new List<string>(); //Uygulamayı kapattığımız anda yok olacak çünkü static olarak tutuyoruz.
        private static int ClientCount { get; set; } = 0; //Bağlı olan client'ların sayısı
        public static int TeamCount { get; set; }= 3;
        private readonly AppDbContext _context;

        public MyHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendProduct(Product p)
        {
            Clients.All.SendAsync("RecieveProduct", p);
        }

        //Client'ların çağıracağı methodları oluşturacağız. Asenkron olarak

        public async Task SendName(string name)
        {
            //Clients property'si üzerinden clientlardaki methodların çalışması için istek göndereceğiz.
            if (Names.Count>=TeamCount)
            {
                await Clients.Caller.SendAsync("Error", $"Konuşmada en fazla {TeamCount} mesaj olabilir.");
            }
            else
            {
                Names.Add(name);
                await Clients.All.SendAsync("RecieveName", name); //ALL bağlı olan tüm client'lara haber gönderir. Sendasync methodu clientlardaki methodu çalıştırma işlemi yapıyor.
                                                                  //Client'lar sadece RecieveMessage methoduna subscribe olmuşsa çalışacaktır.
            }
        }


        public async Task GetNames()
        {
            await Clients.All.SendAsync("RecieveNames", Names);
        }

        public async override Task OnConnectedAsync()
        {
            ClientCount++;
            await Clients.All.SendAsync("RecieveClientCount", ClientCount);
            await base.OnConnectedAsync(); 
        }

        public async Task AddToGroup(string teamName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, teamName);
        }

        public async Task SendNameByGroup(string Name, string teamName)
        {
            var team = _context.Teams.Where(x => x.Name == teamName).FirstOrDefault();

            if (team!=null)
            {
                team.Users.Add(new User { Name = Name }); 
            }
            else
            {
                var newTeam = new Team { Name = teamName };
                newTeam.Users.Add(new User { Name = Name });
                _context.Teams.Add(newTeam);

            }
            await _context.SaveChangesAsync();
            await Clients.Group(teamName).SendAsync("RecieveMessageByGroup", Name, team.Id);
        }

        public async Task GetNamesByGroup()
        {
            var teams = _context.Teams.Include(x => x.Users).Select(x => new
            {
                teamId = x.Id,
                Users=x.Users.ToList()
            });

            await Clients.All.SendAsync("RecieveNamesByGroup", teams);
        }

        public async Task RemoveToGroup(string teamName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, teamName);
        }



        public async override Task OnDisconnectedAsync(Exception exception)
        {
            ClientCount--;
            await Clients.All.SendAsync("RecieveClientCount", ClientCount);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
