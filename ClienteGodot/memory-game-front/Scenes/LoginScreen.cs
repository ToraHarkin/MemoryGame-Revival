using Godot;
using System;

public partial class LoginScreen : Control
{
	// Variables para el menú desplegable
	private Panel _menuPanel;
	private TextureButton _menuButton;
	
	// Variables para los botones centrales
	private Button _btnIniciarSesion;
	private Button _btnRegistrarse;
	private LinkButton _btnInvitado; // Usamos LinkButton por el subrayado que pusiste

	public override void _Ready()
	{
		// 1. Enlazamos los nodos del menú de hamburguesa
		_menuPanel = GetNode<Panel>("MenuPanel");
		_menuButton = GetNode<TextureButton>("MenuButton");

		// 2. Enlazamos los botones dentro de tu contenedor
		// Nota: Godot usa las rutas exactas del panel de la izquierda
		_btnIniciarSesion = GetNode<Button>("panel vertical/IniciarSesion");
		_btnRegistrarse = GetNode<Button>("panel vertical/Registrarse");
		_btnInvitado = GetNode<LinkButton>("panel vertical/Continuar como Invitado");

		// 3. Conectamos los clics a las funciones correspondientes
		_menuButton.Pressed += OnMenuButtonPressed;
		_btnIniciarSesion.Pressed += OnLoginPressed;
		_btnRegistrarse.Pressed += OnRegisterPressed;
		_btnInvitado.Pressed += OnGuestPressed;

		// Nos aseguramos de que el menú lateral empiece escondido
		_menuPanel.Visible = false;
	}

	private void OnMenuButtonPressed()
	{
		// Alterna la visibilidad del menú
		_menuPanel.Visible = !_menuPanel.Visible;
	}

	private void OnLoginPressed()
	{
		GD.Print("Navegando al Formulario de Login...");
	GetTree().ChangeSceneToFile("res://Scenes/login_form.tscn");
	}

	private void OnRegisterPressed()
	{
		GD.Print("Navegando al Formulario de Registro...");
		// Aquí luego pondremos el cambio de escena
	}

	private void OnGuestPressed()
	{
		GD.Print("Lógica de invitado pendiente...");
	}
}
