document
    .getElementById("loginForm")
    .addEventListener("submit", async (event) => {
        event.preventDefault();

        const form = event.target;

        const patient = {
            login: form.login.value,
            password: form.password.value
        };

        const response = await fetch('/api/v1/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(patient)
        })

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
            message.innerText = `Неверно введен логин или пароль.`;
        }
    });
