$(document).ready(function () {

    // ==========================================
    // 1. MAGIA PARA TRADUCIR Y ESTILIZAR IDENTITY
    // ==========================================

    // Darle nuestro diseño negro/rojo a formularios rebeldes como el de Olvidé Contraseña
    $("main .row > .col-md-4 > form").addClass("gym-form");

    // Traducir títulos internos
    $("#account h2").text("INICIAR SESIÓN");
    $("#registerForm h2").text("CREAR CUENTA NUEVA");

    // Traducir campos de texto
    $("label[for='Input_Email']").text("Correo Electrónico");
    $("label[for='Input_Password']").text("Contraseña");
    $("label[for='Input_ConfirmPassword']").text("Confirmar Contraseña");

    // El checkbox es un caso especial porque el texto no está dentro de un label normal
    if ($("label[for='Input_RememberMe']").length) {
        $("label[for='Input_RememberMe']").contents().last().replaceWith(" ¿Recordarme?");
    }

    // Traducir botones principales
    $("#account button[type='submit']").text("ENTRAR");
    $("#registerForm button[type='submit']").text("REGISTRARSE");
    $("button:contains('Reset Password')").text("RECUPERAR CONTRASEÑA");

    // Traducir enlaces inferiores
    $("#forgot-password").text("¿Olvidaste tu contraseña?");
    $("#register").text("Crear una cuenta nueva");

    // Título dinámico para la página de Olvidé Contraseña
    if (window.location.href.indexOf("ForgotPassword") > -1) {
        $(".gym-form").prepend("<h2 class='gym-title'>RECUPERAR CONTRASEÑA</h2><p class='text-white mb-4' style='font-size:0.85rem;'>Ingresa tu correo y te enviaremos las instrucciones.</p>");
    }

    // ==========================================
    // 2. MAGIA PARA LOS SABORES EN DETALLE.CSHTML
    // ==========================================

    // Si existen botones de sabor en la pantalla, activamos esta lógica
    if ($('.flavor-btn').length > 0) {
        $('.flavor-btn').on('click', function () {

            // 1. Quitar la clase "active-flavor" de todos los botones
            $('.flavor-btn').removeClass('active-flavor');

            // 2. Ponerle la clase al botón que le dimos clic
            $(this).addClass('active-flavor');

            // 3. Leer los datos ocultos (Stock y ID)
            let stockReal = $(this).attr('data-stock');
            let varianteId = $(this).attr('data-id');

            // 4. Actualizar el texto del inventario con estilo Kinetic
            $('#stockText')
                .html(`<i class="bi bi-check-circle-fill text-success me-1"></i> INVENTARIO VERIFICADO: ${stockReal} UNIDADES`)
                .removeClass() // borra clases viejas
                .addClass('text-white fw-bold small text-uppercase')
                .css('letter-spacing', '1px');

            // 5. Guardar el ID en el formulario y prender el botón
            $('#varianteSeleccionada').val(varianteId);
            $('#btnAgregarCarrito').prop('disabled', false);
        });
    }

    // ==========================================
    // 3. MAGIA PARA EL SIDE CART Y LA BURBUJA (MENÚ LATERAL)
    // ==========================================

    // Función para ir a buscar el número real a la base de datos
    function actualizarContadorCarrito() {
        $.get('/Carrito/ObtenerContador', function (total) {
            const badge = $('#cartBadge');
            if (badge.length) {
                badge.text(total); // Cambiamos el texto al total real

                // Efecto visual táctico: la burbujita hace un "latido"
                badge.css('transform', 'scale(1.5)');
                setTimeout(() => badge.css('transform', 'scale(1)'), 200);
            }
        });
    }

    // Ejecutar inmediatamente al cargar cualquier página para que el número sea real, no "0"
    actualizarContadorCarrito();

    // Función Helper para refrescar los mini carritos parciales y la burbuja
    function refrescarArsenalLateral() {
        // Llamada 1: Cargar la lista de productos
        $('#cartOffcanvasBody').load('/Carrito/ObtenerMiniCarrito');
        // Llamada 2: Cargar el subtotal y botón
        $('#cartFooterContent').load('/Carrito/ObtenerMiniCarritoFooter');
        // Llamada 3: Actualizar la burbujita roja en el menú principal
        actualizarContadorCarrito();
    }

    // Interceptar el clic del botón "Agregar al Arsenal" en Detalle.cshtml
    if ($('#btnAgregarCarrito').length > 0) {

        // Convertimos el botón submit en un botón normal para que no recargue la página
        $('#btnAgregarCarrito').attr('type', 'button');

        $('#btnAgregarCarrito').on('click', function (e) {
            e.preventDefault();

            // 1. Validar que se haya seleccionado un sabor
            const varianteId = $('#varianteSeleccionada').val();
            if (!varianteId || varianteId === "") {
                // Toque de diseño: si no han seleccionado, hacemos que el contenedor vibre visualmente
                $('#stockContainer').addClass('text-danger fw-black');
                setTimeout(() => $('#stockContainer').removeClass('text-danger fw-black'), 1000);
                return;
            }

            // 2. Extraer datos del formulario (varianteId, productoId, cantidad)
            const form = $(this).closest('form');
            const data = form.serialize();

            // 3. Toque visual: Desactivar botón temporalmente y cambiar texto
            const originalText = $(this).html();
            $(this).prop('disabled', true).html('<i class="bi bi-clock me-2"></i> AGREGANDO...');

            // 4. ENVÍO AJAX (Comunicación silenciosa con CarritoController.cs)
            $.post('/Carrito/Agregar', data, function (response) {

                if (response.success) {
                    // ¡ÉXITO! Refrescar panel y contador
                    refrescarArsenalLateral();

                    // Abrir el menú lateral con Bootstrap JS API
                    const cartElement = document.getElementById('cartOffcanvas');
                    if (cartElement) {
                        const cartOffcanvas = new bootstrap.Offcanvas(cartElement);
                        cartOffcanvas.show();
                    }
                } else {
                    alert('Atención: ' + response.message);
                }

                // Restaurar el botón a la normalidad
                $('#btnAgregarCarrito').prop('disabled', false).html(originalText);
            }).fail(function () {
                // Si falla (ej. sin sesión)
                alert("Debes iniciar sesión para agregar armamento a tu arsenal.");
                window.location.href = "/Identity/Account/Login";
            });
        });
    }

    // Interceptar el clic del botón eliminar (el bote de basura) dentro del Menú Lateral
    $(document).on('click', '.remove-item', function (e) {
        e.preventDefault();
        const itemId = $(this).attr('data-id');

        // Bloquear el botón temporalmente para que no den doble clic
        $(this).prop('disabled', true).html('<i class="bi bi-hourglass"></i>');

        // ENVÍO AJAX ELIMINAR
        $.post('/Carrito/Eliminar', { id: itemId }, function (response) {
            if (response.success) {
                refrescarArsenalLateral();
            } else {
                alert("Hubo un error al eliminar el suplemento.");
            }
        });
    });

    // Interceptar el clic de los botones + y - en el Menú Lateral
    $(document).on('click', '.btn-update-qty', function (e) {
        e.preventDefault();

        const itemId = $(this).attr('data-id');
        const operacion = $(this).attr('data-op');

        // Bloqueamos el botón temporalmente
        const boton = $(this);
        boton.prop('disabled', true);

        // ENVÍO AJAX PARA ACTUALIZAR CANTIDAD
        $.post('/Carrito/ActualizarCantidad', { id: itemId, operacion: operacion }, function (response) {
            if (response.success) {
                refrescarArsenalLateral();
            } else {
                alert("Error al actualizar la cantidad.");
                boton.prop('disabled', false);
            }
        });
    });

    // Opcional: Cada que el usuario abra el menú lateral manualmente, refrescamos la info.
    const cartOffcanvasEl = document.getElementById('cartOffcanvas');
    if (cartOffcanvasEl) {
        cartOffcanvasEl.addEventListener('show.bs.offcanvas', function () {
            refrescarArsenalLateral();
        });
    }

});