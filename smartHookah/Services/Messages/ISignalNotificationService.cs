using smartHookah.Models.Db;

namespace smartHookah.Services.Messages
{
    public interface ISignalNotificationService
    {
        void OnlineDevice(string code);

        void ReservationChanged(Reservation reservation);

        void ReservationChanged(int placeId);

        void SessionSettingsChanged(string deviceCode, DeviceSetting setting);
    }
}