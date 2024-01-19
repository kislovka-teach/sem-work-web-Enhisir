using System.Data.Common;
using Database;
using Database.Entities;
using DatabasePreparer;

await CityParser.SaveData();

/*var moscow = Guid.Parse("0c5b2444-70a0-4932-980c-b4dc0d3f02b5");*/

/*var user = new User()
{
    CityId = moscow,
    Name = "misergeev1",
    Login = "enhisir1"
};
user.SetPassword("xdkess2004");
await user.SaveAsync();*/

/*var myUser = await User.GetByLoginAsync("enhisir1");
var user = await User.GetByLoginAsync("NikoImam");
Console.WriteLine(user?.Name);*/

/*var anct = new Announcement()
{
    Title = "title two",
    Description = "Desc",
    Address = "somewhere",
    CityId = moscow,
    Price = 123.00m,
    OwnerId = myUser!.Login,
};
await anct.SaveAsync();*/

/*var anct = await Announcement.GetByIdAsync(Guid.Parse("bdd78252-213a-4884-94c3-e6ffdd2c183c"));
Console.WriteLine(anct!.Title);

var chat = new Chat(anct, myUser!);
await chat.SaveAsync();*/
// var chat = new Chat();