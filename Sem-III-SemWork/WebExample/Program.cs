using WebExample;
using WebHelper;

var app = new HttpWebApp("localhost", 5000);
app.AddComponent(new NCmp("ncmp"));
await app.RunAsync();
