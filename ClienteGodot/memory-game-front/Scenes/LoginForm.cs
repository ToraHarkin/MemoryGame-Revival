using Godot;
using System;
using System.Text;

public partial class LoginForm : Control
{
	private LineEdit _usuarioInput;
	private LineEdit _passwordInput;
	private Button _btnLogin;
	private LinkButton _btnAtras;
	
	private Panel _menuPanel;
	private TextureButton _menuButton;
	private HttpRequest _httpRequest;

	public override void _Ready()
	{
		// RUTAS CORREGIDAS según tu panel de Escena (image_4fe634.png)
		_menuButton = GetNode<TextureButton>("MenuButton");
		_menuPanel = GetNode<Panel>("MenuPanel");
		
		// El botón Atrás y los inputs están dentro de 'Panel' y luego 'panel vertical'
		_btnAtras = GetNode<LinkButton>("Panel/panel vertical/Atras");
		_btnLogin = GetNode<Button>("Panel/panel vertical/IniciarSesion");
		_usuarioInput = GetNode<LineEdit>("Panel/panel vertical/LineEdit");
		_passwordInput = GetNode<LineEdit>("Panel/panel vertical/LineEdit2");

		_httpRequest = new HttpRequest();
		AddChild(_httpRequest);

		// Conectar señales
		_menuButton.Pressed += OnMenuButtonPressed;
		_btnAtras.Pressed += OnAtrasPressed;
		_btnLogin.Pressed += OnLoginPressed;
		_httpRequest.RequestCompleted += OnRequestCompleted;

		_menuPanel.Visible = false;
		
		GD.Print("Escena de Login lista y nodos conectados.");
	}

	private void OnMenuButtonPressed()
	{
		_menuPanel.Visible = !_menuPanel.Visible;
		GD.Print("Menú hamburguesa: " + (_menuPanel.Visible ? "Abierto" : "Cerrado"));
	}

	private void OnAtrasPressed()
	{
		GD.Print("Regresando a la pantalla de selección...");
		GetTree().ChangeSceneToFile("res://Scenes/login_screen.tscn");
	}

	private void OnLoginPressed()
	{
		// Tu lógica de login con la URL de Luis (http://localhost:5000)
		string json = $"{{\"Username\":\"{_usuarioInput.Text}\", \"Password\":\"{_passwordInput.Text}\"}}";
		string[] headers = new string[] { "Content-Type: application/json" };
		_httpRequest.Request("http://localhost:5000/api/auth/login", headers, HttpClient.Method.Post, json);
	}

	private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		GD.Print("Respuesta del servidor: " + responseCode);
	}
}
