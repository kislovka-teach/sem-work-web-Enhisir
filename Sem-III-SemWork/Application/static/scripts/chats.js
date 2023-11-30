async function getMyChats()
{
    let response = await fetch("/api/v1/user/me/chats");
    if (response.ok)
        return await response.json();
    return [];
}

let lastTimeCalled = undefined
async function getMessages(chatId, time=false)
{
    let requestString = `/api/v1/chat/id/${chatId}/messages`;
    if (time)
        requestString += `?filterDateTime=${lastTimeCalled}`;
    
    let response = await fetch(requestString);
    lastTimeCalled = new Date().toUTCString();
    if (response.ok)
        return await response.json();
    return [];
}

let lastInterval = null
async function fillChatsOnPage(chats)
{
    const me = await getCurrentUser();
    const tabList = document.getElementById("list-tab");
    const tabContentList = document.getElementById("nav-tabContent");
    for (const chat of chats)
    {
        const interlocutor = 
            me.login === chat.consumer.login 
            ? chat.announcement.owner 
            : chat.consumer;
        const tab = document.createElement("a");
        
        tab.className = "list-group-item list-group-item-action";
        tab.id = `list-${chat.id}`;
        tab.href = `#chat-${chat.id}`;
        tab.setAttribute("data-toggle", "list");
        tab.setAttribute("role", "tab");
        
        tab.innerHTML = `<div class="d-flex w-100 justify-content-between">
                             <div class="user-circle mr-3"><H1 id="userblockLetter">${interlocutor.name.charAt(0)}</H1></div>
                             <div class="d-flex flex-column flex-grow-1 my-auto">
                                 <span class="user-title highlight" id="userblockName">${interlocutor.name}</span>
                                 <span class="user-subtitle" id="announcementName">${chat.announcement.title}</span>
                             </div>
                         </div>`;
        tabList.append(tab);
        
        const tabContent = document.createElement("div");
        tabContent.className = "tab-pane fade show";
        tabContent.id = `chat-${chat.id}`;
        tabContent.setAttribute("aria-labelledby", tab.id);
        tabContent.setAttribute("role", "tabpanel");
        tabContent.innerHTML = `<div class="d-flex chat-height-constraint flex-column">
                                    <div class="chat-box scrollable d-flex flex-column flex-grow-1 w-100 px-1"
                                         id="messages-${chat.id}"></div>
                                    <form 
                                    action="#" class="bg-light mt-auto" 
                                    id="form-${chat.id}"
                                    onsubmit="sendMessage(event, this, '${chat.id}', '${chat.announcement.id}')">
                                        <div class="input-group">
                                            <input type="text"
                                                   placeholder="Type a message"
                                                   aria-describedby="button-addon2"
                                                   class="form-control rounded-0 border-0 py-4 bg-light"
                                                   name="content">
                                            <div class="input-group-append">
                                                <button type="submit" class="btn btn-success">
                                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
                                                         fill="currentColor" class="bi bi-send" viewBox="0 0 16 16">
                                                        <path d="M15.854.146a.5.5 0 0 1 .11.54l-5.819 14.547a.75.75 0 0 1-1.329.124l-3.178-4.995L.643 7.184a.75.75 0 0 1 .124-1.33L15.314.037a.5.5 0 0 1 .54.11ZM6.636 10.07l2.761 4.338L14.13 2.576 6.636 10.07Zm6.787-8.201L1.591 6.602l4.339 2.76 7.494-7.493Z"/>
                                                    </svg>
                                                </button>
                                            </div>
                                        </div>
                                    </form>
                                </div>`;
        tabContentList.append(tabContent);
        
        $(`#list-tab #list-${chat.id}`).on('click', async (event) => {
            event.preventDefault();
            clearInterval(lastInterval);
            
            const chatBox = document.getElementById(`messages-${chat.id}`);
            chatBox.innerHTML = '';
            const messages = await getMessages(chat.id);
            addMessagesToChat(chatBox, messages, interlocutor);
            
            lastInterval = setInterval(async() => {
                let newMessages = await getMessages(chat.id, true);
                addMessagesToChat(chatBox, newMessages, interlocutor);
            }, 3000);
        })
    }
}

async function sendMessage(event, form, chatId, antId)
{
    event.preventDefault();
    const message = {
        chatid: chatId,
        announcementid: antId,
        text: form.content.value,
    }
    
    const response = await fetch("/api/v1/chat/messages/new", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(message)
    });

    form.content.value = "";
}

function addMessagesToChat(chatBox, messages, interlocutor)
{
    for (const msg of messages)
    {
        const msgContainer = document.createElement("div");
        msgContainer.innerText = msg.content;

        msgContainer.className = "w-50 message d-flex";
        msgContainer.classList.add(msg.senderid === interlocutor.login ? "message-other" : "message-mine");
        msgContainer.classList.add(msg.senderid === interlocutor.login ? "mr-auto" : "ml-auto");
        chatBox.append(msgContainer);
    }
}

document.addEventListener("DOMContentLoaded", async () => await getMyChats().then(fillChatsOnPage))