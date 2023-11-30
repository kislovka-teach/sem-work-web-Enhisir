'use strict';

document.addEventListener("DOMContentLoaded", async () => {
    const response = await fetch(`/api/v1/announcement/id/${announcementId}`);
    const item = await response.json();
    
    document.getElementById("title").value = item.title;
    document.getElementById("description").value = item.description;
    document.getElementById("price").value = item.price.toString();
    $("#city").val(item.city.name);
    $("#address").val(item.address);
});

async function editAnnouncement(event) {
    'use strict'
    event.preventDefault();

    const form = event.target;
    
    const announcement = {
        description: form.description.value,
        price: Number.parseFloat(form.price.value),
    };

    const response = await fetch(`/api/v1/announcement/id/${announcementId}/edit`, {
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
        window.location.href = `/announcement/id/${announcementId}`;
    }
    else {
        messageContainer.classList.add("bg-danger");
        message.innerText = `Произошла непредвиденная ошибка.`;
    }
}

document
    .getElementById("announcementForm")
    .addEventListener('submit', editAnnouncement);

const pathElements = window.location.pathname.split("/").filter(i => i);
const announcementId = pathElements[pathElements.length - 2];