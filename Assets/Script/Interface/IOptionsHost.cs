/// <summary>
/// Contrat commun entre MenuNavigator et PauseMenuController
/// pour que OptionsController puisse appeler ReturnToMainMenu() sans connaître son hôte.
/// </summary>
public interface IOptionsHost
{
    void ReturnToPreviousScreen();
}
