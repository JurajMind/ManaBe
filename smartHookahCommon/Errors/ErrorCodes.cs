using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smartHookahCommon.Errors
{
    public static class ErrorCodes
    {
        public static string PlaceNotFound = "PLACE_NOT_FOUND";

        public static string ReservationNotFound = "RESERVATION_NOT_FOUND";

        public static string TableNotFound = "TABLE_NOT_FOUND";

        public static string ReservationConflict = "RESERVATION_CONFLICT";

        public static string ReservationStateRole = "RESERVATION_STATE_ROLE";

        public static string WrongOrderField = "WRONG_ORDER_FIELD";

        public static string InvalidPrivacyType = "INVALID_PRIVACY_TYPE";

        public static string WrongDay = "WRONG_DATE";

        public static string UpdateError = "UPDATE_ERROR";

        public static string DeviceNotFound = "DEVICE_NOT_FOUND";

        public static string DeviceAlreadyAdded = "DEVICE_ALREADY_ADDED";

        public static string SessionNotFound = "SESSION_NOT_FOUND";

        public static string AccessoryNotFound = "ACCESSORY_NOT_FOUND";

        public static string MediaUploadError = "MEDIA_UPLOAD_ERROR";

        public static string ReviewNotFond = "REVIEW_NOT_FOUND";
    }
}
