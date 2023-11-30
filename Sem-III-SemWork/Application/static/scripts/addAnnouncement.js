'use strict';

async function imageToByteArrayAsync(file)
{
    return new Promise((resolve, reject) => {
        try {
            let reader = new FileReader();
            let fileByteArray = [];
            reader.readAsArrayBuffer(file);
            reader.onloadend = (evt) => {
                if (evt.target.readyState === FileReader.DONE) {
                    let arrayBuffer = evt.target.result,
                        array = new Uint8Array(arrayBuffer);
                    for (let byte of array) {
                        fileByteArray.push(byte);
                    }
                }
                resolve(fileByteArray);
            }
        }
        catch (e) {
            reject(e);
        }
    })
}

const getBase64 = file => new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => resolve(reader.result);
    reader.onerror = reject;
});

async function addAnnouncement(event) {
    'use strict'
    event.preventDefault();

    const form = event.target;
    
    const announcement = {
        title: form.title.value,
        description: form.description.value,
        price: Number.parseFloat(form.price.value),
        cityid: $city.suggestions().selection.data.fias_id,
        address: form.address.value,
        image: await getBase64(form.image.files[0])
    };

    const response = await fetch('/api/v1/announcement/new', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(announcement)
    });

    const messageContainer = document.getElementById("resultMessage");
    messageContainer.innerHTML = "";
    messageContainer.classList.remove("bg-primary");
    messageContainer.classList.remove("bg-danger");

    const message = document.createElement("span");
    message.className = "p-2 align-middle";
    messageContainer.append(message);
    if (response.ok) {
        messageContainer.classList.add("bg-primary");
        message.innerText =
            "Успешное сохранение клиента. " +
            "Через пару секунд вы будете " +
            "перенаправлены на главную страницу";
        setTimeout(function () {
            window.location.href = "/";
        }, 2000);
    }
    else {
        messageContainer.classList.add("bg-danger");
        message.innerText =
            `Увы, во время сохранение пользователя ` +
            `произошла непредвиденная ошибка.`;
    }
}

document
    .getElementById("announcementForm")
    .addEventListener('submit', addAnnouncement);


let $city = $("#city");

$city.suggestions({
    token: "06d45098c463e5f156972e42af91eeffdda82c93",
    type: "ADDRESS",
    hint: false,
    bounds: "city-settlement"
});

// улица
$("#address").suggestions({
    token: "06d45098c463e5f156972e42af91eeffdda82c93",
    type: "ADDRESS",
    hint: false,
    bounds: "street, house",
    constraints: $city
});