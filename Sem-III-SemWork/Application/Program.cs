// See https://aka.ms/new-console-template for more information

using Application.Api;
using Application.Frontend;
using WebHelper;

var app = new HttpWebApp("localhost", 5000);
app.AddComponent(new ApiAuthComponent());
app.AddComponent(new ApiCityComponent());
app.AddComponent(new ApiUserComponent());
app.AddComponent(new ApiAnnouncementComponent());
app.AddComponent(new ChatApiComponent());
app.AddComponent(new ApiImageComponent());
app.AddComponent(new ShowMainComponent());
app.AddComponent(new ShowAuthComponent());
app.AddComponent(new ShowAnnouncementComponent());
app.AddComponent(new ShowUserComponent());

await app.RunAsync();
