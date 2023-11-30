using System.Net;
using System.Text.Json;
using Application.Models;
using Database;
using Database.Entities;
using WebHelper.Attributes;
using WebHelper.Components;
using WebHelper.Models;
using WebHelper.Tools;

namespace Application.Api;

public class ApiAuthComponent : ApiComponent
{
    public ApiAuthComponent() : base("auth") {}
    
    [Mapping(Method.POST, "/login")]
    public async Task Login(Context context)
    {
        try
        {
            var loginModel = await JsonSerializer.DeserializeAsync<LoginModel>(
                context.Request.InputStream,
                JsonSerializing.StandardOptions);

            if (loginModel is null)
                throw new Exception();
            loginModel.Login = loginModel.Login.ToLower();

            EntityValidator.Validate(loginModel);
            var maybeUser = await User.GetByLoginAsync(loginModel.Login);

            if (maybeUser is null || !maybeUser.ComparePassword(loginModel.Password))
            {
                ShowUnauthorized(context);
            }
            else
            {
                var session = maybeUser.CreateSession();
                context.Response.Cookies.Add(new Cookie
                {
                    HttpOnly = true,
                    Name = "session-id",
                    Value = session.Id.ToString(),
                    Expires = DateTime.UtcNow.AddDays(30d),
                    Path = "/"
                });

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentLength64 = 0;
            }
        }
        catch (Exception)
        {
            ShowBadRequest(context);
        }
    }
    
    [Mapping(Method.DELETE, "/logout")]
    public Task Logout(Context context)
    {
        context.Response.AppendCookie(
            new Cookie("session-id", null)
            {
                Path = "/",
                Expired = true
            });
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentLength64 = 0;
        return Task.CompletedTask;
    }

    [Mapping(Method.POST, "/register")]
    public async Task Register(Context context)
    {
        try
        {
            var model = await JsonSerializer
                .DeserializeAsync<RegistrationModel>(
                    context.Request.InputStream,
                    JsonSerializing.StandardOptions);

            if (model is null) throw new Exception();
            
            EntityValidator.Validate(model);
            var user = new User
            {
                City = await City.GetByIdAsync(model.CityId) ?? throw new ItemNotFoundException(),
                Login = model.Login.ToLower(),
                Name = model.Name
            };
            user.SetPassword(model.Password);

            await user.SaveAsync();
            context.Response.StatusCode = (int)HttpStatusCode.Created;
            context.Response.ContentLength64 = 0;
        }
        catch (Exception)
        {
            ShowBadRequest(context);
        }
    }
}