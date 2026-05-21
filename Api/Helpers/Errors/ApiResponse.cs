namespace Api.Helpers.Errors;

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public ApiResponse(int statusCode, string? message = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessage(statusCode);
    }

    private static string GetDefaultMessage(int statusCode)
    {
        return statusCode switch
        {
            400 => "Has realizado una peticion incorrecta.",
            401 => "Usuario no autorizado.",
            404 => "El recurso que has intentado solicitar no existe.",
            405 => "Este metodo HTTP no esta permitido en el servidor.",
            409 => "Conflicto de datos en la solicitud.",
            500 => "Error en el servidor. Contacta al administrador.",
            _ => throw new NotImplementedException()
        };
    }
}
