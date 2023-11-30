getAvailable().then(
    async (available) => {
        if (available == null)
            return;
        
        if (/complete|interactive|loaded/.test(document.readyState)) {
            // In case the document has finished parsing, document's readyState will
            // be one of "complete", "interactive" or (non-standard) "loaded".
            await addData(available);
        } else {
            // The document is not ready yet, so wait for the DOMContentLoaded event
            document.addEventListener(
                'DOMContentLoaded',
                async () => { await addData(available) },
                false);
        }
    });

function addData(available)
{
    let row = document.getElementById("itemsRow");
    for (let el of available)
        row.append(getAnnouncementCard(el));
}

async function getAvailable()
{
    let response = await fetch("/api/v1/announcement/catalog");
    if (!response.ok)
    {
        document.getElementById("searchTitle").innerText = "Объявления не найдены"
        return null;
    }

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
    thumb.className = "card-img-top embed-responsive-item";
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