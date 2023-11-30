let validations = [
    validatePassword,
    validatePasswordRepeat,
    validateLogin,
];

async function validateLogin(event) {
    'use strict'
    event.preventDefault();

    const loginInput = document.getElementById('login');
    const loginFeedback = document.getElementById('loginFeedback');

    if (!loginInput.value.match(/^([a-zA-Z\d_-]+)$/))
    {
        loginInput.classList.remove('is-valid');
        loginInput.classList.add('is-invalid');
        loginFeedback.innerText = "логин может содержать строчные и прописные буквы, цифры, а также символы _, -";

        return false;
    }

    let response = await fetch(`/api/v1/user/${loginInput.value}`, {
        method: 'GET',
    });

    if (response.ok) {
        loginInput.classList.remove('is-valid');
        loginInput.classList.add('is-invalid');
        loginFeedback.innerText = "такой логин уже используется";
        return false;
    }
    else {
        loginInput.classList.remove('is-invalid');
        loginInput.classList.add('is-valid');
        loginFeedback.innerText = '';
        return true;
    }
}

async function validatePassword(event) {
    'use strict'
    event.preventDefault();

    let isAnyInvalid = false;
    let pwdParameters = [
        {
            regex: /[a-z]/g,
            message: 'Пароль должен содержать строчные буквы'
        },
        {
            regex: /[A-Z]/g,
            message: 'Пароль должен содержать прописные буквы'
        },
        {
            regex: /[0-9]/g,
            message: 'Пароль должен содержать числа'
        },
        {
            regex: /[/\\.^*+?!()\[\]{}|]/g,
            message: 'Пароль должен содержать спецсимволы'
        },
        {
            regex: /.{8,}/g,
            message: 'Пароль должен иметь длину не менее 8 символов'
        }
    ];

    let pwdInput = document.getElementById('password');
    let messages = [];
    for (let i = 0; i < pwdParameters.length; i++)
    {
        if (!pwdInput.value.match(pwdParameters[i].regex)) {
            messages.push(pwdParameters[i].message);
            isAnyInvalid = true;
        }
    }

    let pwdFeedback = document.getElementById('passwordFeedback');
    if (isAnyInvalid) {
        pwdInput.classList.remove('is-valid');
        pwdInput.classList.add('is-invalid');
        pwdFeedback.innerText = messages.join(';');
    } else {
        pwdInput.classList.remove('is-invalid');
        pwdInput.classList.add('is-valid');
        pwdFeedback.innerText = '';
    }

    return !isAnyInvalid;
}

async function validatePasswordRepeat(event) {
    'use strict'
    event.preventDefault();

    const pwdRepeatFeedback = document.getElementById('passwordRepeatFeedback');

    const password = document.getElementById('password');
    const passwordRepeat = document.getElementById('passwordRepeat');
    if (password.value !== passwordRepeat.value) {
        passwordRepeat.classList.remove('is-valid');
        passwordRepeat.classList.add('is-invalid');
        pwdRepeatFeedback.innerText = "пароли не совпадают";
        return false;
    }
    else {
        passwordRepeat.classList.remove('is-invalid');
        passwordRepeat.classList.add('is-valid');
        pwdRepeatFeedback.innerText = '';
        return true;
    }
}

async function sendRegister(event) {
    'use strict'
    event.preventDefault();

    const form = event.target;

    const user = {
        name: form.name.value,
        cityid: form.selectCity.value,
        login: form.login.value,
        password: form.password.value
    };

    const response = await fetch('/api/v1/auth/register', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(user)
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
    .getElementById("password")
    .addEventListener("keyup", validatePassword);

document
    .getElementById("password")
    .addEventListener("keyup", validatePasswordRepeat);

document
    .getElementById("passwordRepeat")
    .addEventListener("keyup", validatePasswordRepeat);

document
    .getElementById("registrationForm")
    .addEventListener('submit', async (event) => {
        'use strict'

        const asyncSome = async (arr, predicate) => {
            for (let e of arr) {
                if (await predicate(e)) return true;
            }
            return false;
        };

        if (await asyncSome(validations, async func => !(await func(event))) || !event.target.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        } else {
            event.target.classList.add('was-validated')
            await sendRegister(event);
        }
    });