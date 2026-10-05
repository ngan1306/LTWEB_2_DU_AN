const form = document.querySelector("#auth-form");
const loginTab = document.querySelector("#login-tab");
const registerTab = document.querySelector("#register-tab");
const nameField = document.querySelector("#name-field");
const birthField = document.querySelector("#birth-field");
const message = document.querySelector("#auth-message");
const submitButton = document.querySelector("#auth-submit");
const title = document.querySelector("#auth-title");
const kicker = document.querySelector("#auth-kicker");
const subtitle = document.querySelector("#auth-subtitle");
let mode = "login";

function setMode(nextMode) {
  mode = nextMode;
  const registering = mode === "register";
  loginTab.classList.toggle("is-active", !registering);
  registerTab.classList.toggle("is-active", registering);
  loginTab.setAttribute("aria-selected", String(!registering));
  registerTab.setAttribute("aria-selected", String(registering));
  nameField.hidden = !registering;
  birthField.hidden = !registering;
  for (const field of [nameField, birthField]) {
    const input = field.querySelector("input");
    input.disabled = !registering;
    input.required = registering;
  }
  form.elements.password.autocomplete = registering ? "new-password" : "current-password";
  title.textContent = registering ? "Tạo tài khoản." : "Chào bạn.";
  kicker.textContent = registering ? "BẮT ĐẦU MỘT CÂU CHUYỆN MỚI" : "RẤT VUI ĐƯỢC GẶP LẠI";
  subtitle.textContent = registering ? "Tạo hồ sơ Kiss và mở đầu một cuộc gặp gỡ." : "Đăng nhập để tiếp tục hành trình gặp gỡ.";
  submitButton.innerHTML = `${registering ? "Tạo tài khoản" : "Đăng nhập"} <span aria-hidden="true">↗</span>`;
  message.hidden = true;
  message.classList.remove("is-success");
}

async function submitAuth(event) {
  event.preventDefault();
  if (!form.reportValidity()) return;

  const values = new FormData(form);
  const registering = mode === "register";
  const payload = {
    email: values.get("email").trim(),
    password: values.get("password")
  };

  if (registering) {
    payload.name = values.get("name").trim();
    payload.birthDate = values.get("birthDate");
  }

  submitButton.disabled = true;
  submitButton.textContent = "Đang xử lý...";
  message.hidden = true;
  message.classList.remove("is-success");

  try {
    const response = await fetch(`/api/auth/${registering ? "register" : "login"}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });
    const result = await response.json();

    if (!response.ok) {
      throw new Error(result.detail || result.title || "Không thể đăng nhập. Vui lòng thử lại.");
    }

    localStorage.setItem("kiss.accessToken", result.accessToken);
    localStorage.setItem("kiss.profile", JSON.stringify(result.profile));
    message.textContent = registering ? "Tạo tài khoản thành công. Chào mừng bạn đến với Kiss!" : `Đăng nhập thành công. Chào ${result.profile.name}!`;
    message.classList.add("is-success");
    message.hidden = false;
  } catch (error) {
    message.textContent = error.message || "Không thể kết nối máy chủ. Vui lòng thử lại.";
    message.hidden = false;
  } finally {
    submitButton.disabled = false;
    submitButton.innerHTML = `${registering ? "Tạo tài khoản" : "Đăng nhập"} <span aria-hidden="true">↗</span>`;
  }
}

loginTab.addEventListener("click", () => setMode("login"));
registerTab.addEventListener("click", () => setMode("register"));
form.addEventListener("submit", submitAuth);
