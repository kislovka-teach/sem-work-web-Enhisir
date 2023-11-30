searchForUser().then(async (available) => {
    if (available == null || available.length === 0)
        return;

    if (/complete|interactive|loaded/.test(document.readyState))
    {
        // In case the document has finished parsing, document's readyState will
        // be one of "complete", "interactive" or (non-standard) "loaded".
        await addData(available);
    } 
    else 
    {
        // The document is not ready yet, so wait for the DOMContentLoaded event
        document.addEventListener(
            'DOMContentLoaded', 
            () => addData(available), 
            false);
    }
});

function addData(available)
{
    let row = document.getElementById("itemsRow");
    for (let el of available)
        row.append(getAnnouncementCard(el));
}

async function fulfillUserBlock(login)
{
    const currentUser = await getCurrentUser();
    
    const response = await fetch(`/api/v1/user/id/${login}`);
    const user = await response.json();
    
    const nameLink = document.getElementById("userblockName");
    nameLink.innerText = user.name;
    nameLink.href = `/user/${login}`;
    document.getElementById("userblockLetter").innerText = user.name.charAt(0).toUpperCase();
    
    if (currentUser != null && user.login === currentUser.login)
    {
        const userContainer = document.getElementById("userBlockContainer");
        
        const chatsLink = document.createElement("a")
        chatsLink.innerText = "Сообщения";
        chatsLink.href = "/chats";
        userContainer.append(chatsLink);

        const newAnnouncement = document.createElement("a")
        newAnnouncement.innerText = "Новое объявление";
        newAnnouncement.href = "/announcement/new";
        userContainer.append(newAnnouncement);
    }
}

async function searchForUser()
{
    let login = window.location.pathname.split("/").filter(i => i).pop();
   
    let response = await fetch(`/api/v1/user/id/${login}/announcements`);

    if (response.status === 400)
    {
        document.getElementById("searchTitle").innerText = "Неверный запрос";
        return null;
    }

    await fulfillUserBlock(login);
    return response.json();
}

function getAnnouncementCard(announcement)
{
    let column = document.createElement("div");
    column.className = "col mb-3 card-size";

    let container = document.createElement("div");
    container.className = "item-card card-size card shadow-sm h-100";
    if (announcement.isprivileged)
        container.classList.add("privileged-border");
    column.append(container);

    const thumbContainer = document.createElement("div");
    thumbContainer.className = "embed-responsive embed-responsive-4by3";
    container.append(thumbContainer);
    
    let thumb = document.createElement("img");
    thumb.className = "card-img-top embed-responsive-item rounded";
    thumb.src = `/api/v1/image/${announcement.id}/thumb`;
    thumbContainer.append(thumb);

    let card = document.createElement("div");
    card.className = "card-body";
    container.append(card);

    let price = document.createElement("h4");
    price.className = "card-title";
    price.innerText = announcement.price;
    card.append(price);

    let title = document.createElement("a");
    title.className = "d-block card-link text-truncate";
    title.innerText = announcement.title;
    title.href = `/announcement/id/${announcement.id}`;
    card.append(title);

    return column;
}