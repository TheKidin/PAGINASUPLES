// MAGIA PARA TRADUCIR Y ESTILIZAR IDENTITY EN EL PIA
$(document).ready(function () {
    // 1. Darle nuestro diseño negro/rojo a formularios rebeldes como el de Olvidé Contraseña
    $("main .row > .col-md-4 > form").addClass("gym-form");

    // 2. Traducir títulos internos
    $("#account h2").text("INICIAR SESIÓN");
    $("#registerForm h2").text("CREAR CUENTA NUEVA");

    // 3. Traducir campos de texto
    $("label[for='Input_Email']").text("Correo Electrónico");
    $("label[for='Input_Password']").text("Contraseña");
    $("label[for='Input_ConfirmPassword']").text("Confirmar Contraseña");

    // El checkbox es un caso especial porque el texto no está dentro de un label normal
    if ($("label[for='Input_RememberMe']").length) {
        $("label[for='Input_RememberMe']").contents().last().replaceWith(" ¿Recordarme?");
    }

    // 4. Traducir botones principales
    $("#account button[type='submit']").text("ENTRAR");
    $("#registerForm button[type='submit']").text("REGISTRARSE");
    $("button:contains('Reset Password')").text("RECUPERAR CONTRASEÑA");

    // 5. Traducir enlaces inferiores
    $("#forgot-password").text("¿Olvidaste tu contraseña?");
    $("#register").text("Crear una cuenta nueva");

    // 6. Título dinámico para la página de Olvidé Contraseña
    if (window.location.href.indexOf("ForgotPassword") > -1) {
        $(".gym-form").prepend("<h2 class='gym-title'>RECUPERAR CONTRASEÑA</h2><p class='text-white mb-4' style='font-size:0.85rem;'>Ingresa tu correo y te enviaremos las instrucciones.</p>");
    }
});