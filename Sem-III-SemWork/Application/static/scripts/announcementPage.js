document.addEventListener("DOMContentLoaded", async function() {
    let id = window.location.pathname.split("/").filter(i => i).pop();
    let response = await fetch(`/api/v1/announcement/id/${id}`);
    while (!response.ok)
        response = await fetch(`/api/v1/announcement/id/${id}`);
    
    const item = await response.json();
    fulfillAnnouncement(item);
    
    response = await fetch(`/api/v1/user/id/${item.ownerid}`);
    while (!response.ok)
        response = await fetch(`/api/v1/announcement/id/${id}`);
    
    const owner = await response.json();
    fulfillUserBlock(owner);

    const btnGroup = document.getElementById("buttonGroup");
    const currentUser = await getCurrentUser();
    if (currentUser != null && owner.login === currentUser.login)
    {
        const editBtn = document.createElement("a");
        editBtn.setAttribute("type", "button");
        editBtn.className = "flex-grow-1 btn btn-success";
        editBtn.innerText = "Редактировать";
        editBtn.href = `/announcement/id/${id}/edit`
        
        const deleteBtn = document.createElement("a");
        deleteBtn.setAttribute("type", "button");
        deleteBtn.className = "ml-2 btn btn-danger";
        deleteBtn.innerText = "Удалить";
        deleteBtn.onclick = async () => {
            const response = await fetch(`/api/v1/announcement/id/${id}/remove`, 
                { method: 'POST', });
            
            if (response.ok)
                window.location.href = "/";
        }

        const changePrivilegeBtn = document.createElement("a");
        changePrivilegeBtn.setAttribute("type", "button");
        changePrivilegeBtn.className = "mt-2 btn btn-info";
        changePrivilegeBtn.innerText = item.isprivileged ? "Снять продвижение" : "Продвинуть";
        changePrivilegeBtn.onclick = async () => {
            await fetch(`/api/v1/announcement/id/${id}/change_privilege`, { method: 'POST', });
            window.location.reload(); 
        }

        btnGroup.append(editBtn);
        btnGroup.append(changePrivilegeBtn);
        btnGroup.append(deleteBtn);
    }
    else
    {
        const chatBtn = document.createElement("a");
        chatBtn.setAttribute("type", "button");
        chatBtn.className = "btn btn-success w-100";
        chatBtn.innerText = "Написать продавцу";
        chatBtn.href = "#modalNewMessage";
        chatBtn.setAttribute("data-toggle", "modal");
        chatBtn.setAttribute("data-target", "#modalNewMessage");
        
        btnGroup.append(chatBtn);
    }
    
    document.getElementById("modalSendMsgBtn").addEventListener("click", async (event) => {
        event.preventDefault();
            
        if (currentUser == null) {
            alert("Авторизация не пройдена");
            return;
        }

        if (owner.login === currentUser.login) {
            alert("Вы не можете слать сообщение самому себе");
            return;
        }

        let msgTxt = document.getElementById("messageText");

        if (msgTxt.value == null)
            return;
            
        const msg = {
            announcementid: item.id,
            text: msgTxt.value
        };
        
        await fetch("/api/v1/chat/messages/new", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(msg)
        });
        
        window.location.href = "/chats";
    });
});

function fulfillAnnouncement(announcement)
{
    document.getElementById("itemTitle").innerText = announcement.title; // localhost:5000/announcement/id/0858be8c-faf9-49fb-bc17-dbc18246037c
    document.getElementById("itemDescription").innerText = announcement.description;
    document.getElementById("itemPrice").innerText = `${announcement.price} ₽`;
    document.getElementById("itemAddress").innerText = announcement.address;
    const image = document.getElementById("itemImage");
    image.src = `/api/v1/image/${announcement.id}`;
    
    if (announcement.isprivileged)
    {
        image.classList.add("privileged-border");
    }
}

function fulfillUserBlock(user)
{
    const nameLink = document.getElementById("userblockName");
    nameLink.innerText = user.name;
    nameLink.href = `/user/${user.login}`;
    document.getElementById("userblockLetter").innerText = user.name.charAt(0).toUpperCase();
}