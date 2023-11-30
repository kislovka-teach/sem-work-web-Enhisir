"use strict";

const setCookie = (name, value, days = 30, path = '/') => {
    const expires = new Date(Date.now() + days * 864e5).toUTCString()
    document.cookie = name + '=' + encodeURIComponent(value) + '; expires=' + expires + '; path=' + path
}

const getCookie = (name) => {
    return document.cookie.split('; ').reduce((r, v) => {
        const parts = v.split('=')
        return parts[0] === name ? decodeURIComponent(parts[1]) : r
    }, '')
}

const deleteCookie = (name, path) => {
    setCookie(name, '', -1, path)
}

async function getCurrentUser()
{
    let responce = await fetch("/api/v1/user/me", {
        method: 'GET',
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    });

    if (responce.ok)
        return await responce.json();
    else
        return null;
}

async function selectCityChange(user) {
    
    const cityCookie = getCookie("current-city");
    let city;
    
    if (cityCookie === "")
    {
        city = user == null 
            ? {
                id: "0c5b2444-70a0-4932-980c-b4dc0d3f02b5",
                name: "г Москва"
            }
            : user.city;
        
        setCookie("current-city", JSON.stringify(city));
    }
    else
    {
        city = JSON.parse(cityCookie)
    }
    
    document.getElementById("selectCitySpan").innerText = `${city.name}`;
}

function spawnButtons(user) {
/*    const container = document.createElement("ul");
    container.className = "nav navbar-nav ml-auto"; // mx-2 d-flex flex-row*/

    const navbar = document.getElementById("buttonContainer");
/*    navbar.append(container);*/
    if (user == null) {
        const navItem1 = document.createElement("li");
        navItem1.className = "nav-item mx-1 mb-1";
        navItem1.innerHTML = `<a type="button"
                               class="btn btn-primary btn-block"
                               href="/auth/login">Войти</a>`;
        navbar.append(navItem1);

        const navItem2 = document.createElement("li");
        navItem2.className = "nav-item mx-1";
        navItem2.innerHTML = `<a type="button"
                               class="btn btn-primary btn-block"
                               href="/auth/register">Регистрация</a>`;
        navbar.append(navItem2);
    } else {
        const navItem1 = document.createElement("li");
        navItem1.className = "nav-item mx-1 mb-1";
        navbar.append(navItem1);
        const nameLink = document.createElement("a");
        nameLink.className = "navbar-text";
        nameLink.innerText = user.name;
        nameLink.href = `/user/${user.login}`;
        navItem1.append(nameLink);

        const navItem2 = document.createElement("li");
        navItem2.className = "nav-item mx-1";
        navbar.append(navItem2);
        const quitButton = document.createElement("a");
        quitButton.type = "button";
        quitButton.className = "btn btn-primary btn-block";
        quitButton.innerText = "Выйти";
        quitButton.onclick = async () => {
            await fetch('/api/v1/auth/logout', {
                method: 'DELETE',
                headers: {'Content-Type': 'application/json;charset=utf-8'}
            }).then(() => window.location.reload());
        };
        navItem2.append(quitButton);
    }
}

async function configureSelectCity() {
    let response = await fetch("/api/v1/city/all");
    let options = (await response.json()).map(cityJson => {
        const option = document.createElement("option");
        option.value = cityJson["id"];
        option.text = cityJson["name"];
        return option;
    });

    const selectCityCollection = document.getElementsByName("selectCity");
    selectCityCollection.forEach(element =>
    {
        options.forEach(cityEl => {
            element.append(cityEl.cloneNode(true));
        });
    });
    $('select').selectpicker();
}

document.addEventListener(
    "DOMContentLoaded",
    async () => {
        let user = await getCurrentUser();

        spawnButtons(user);
        await selectCityChange(user);
    });

document.addEventListener("DOMContentLoaded", configureSelectCity);

document.getElementById("selectCityButton").addEventListener("click", () => {
    const selectCity = document.getElementById("selectCity")
    const selectedOption = selectCity.options[selectCity.selectedIndex];
    
    const city = {
        "id": selectedOption.value,
        "name": selectedOption.innerText
    }
    setCookie("current-city", JSON.stringify(city));
    $('#modalSelectCity').modal('toggle');
    window.location.reload();
})

document.getElementById("searchBtn").addEventListener("click", () => {
    const search = document.getElementById("searchInput").value ;
    window.location.href = `/search?` + new URLSearchParams({title: search,});
})

