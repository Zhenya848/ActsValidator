namespace UserService.Domain.Shared
{
    public class Constants
    {
        public const int MAX_LOW_TEXT_LENGTH = 100;
        public const int MAX_HIGH_TEXT_LENGTH = 1000;
    }

    public class UserConstants
    {
        public const int TRIAL_USER_BALANSE = 1;

        public const string AllowedUsernameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" +
            "-._@+$!№=#%:^&?*();/ " +
            "абвгдеёжзийклмнопрстуфхцчшщъыьэюя" +
            "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
    }
}
