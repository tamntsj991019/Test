using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ConstData
{

    #region Constants of Entities
    public class ConstRole
    {
        public const string ADMIN = "ADMIN";
        public const string CUSTOMER = "CUSTOMER";
        public const string EMPLOYEE = "EMPLOYEE";

        public const string EMPCUS = "EMPLOYEE, CUSTOMER";
    }

    public class ConstBookingStatus
    {
        public const string CREATE_SUCCESS = "CREATE_SUCCESS";
        public const string PAYMENT_WAITING = "PAYMENT_WAITING";
        public const string BALANCE_NOT_ENOUGH = "BALANCE_NOT_ENOUGH";
        public const string FINDING_EMPLOYEE = "FINDING_EMPLOYEE";
        public const string EMPLOYEE_NOT_FOUND = "EMPLOYEE_NOT_FOUND";
        public const string WAITING = "WAITING";
        public const string CANCELLED = "CANCELLED";
        public const string REJECTED = "REJECTED";
        public const string PROCESSING = "PROCESSING";
        public const string CONFIRM_WAITING = "CONFIRM_WAITING";
        public const string RE_CLEANING = "RE_CLEANING";
        public const string DONE = "DONE";
    }

    public class ConstSetting
    {
        public const string TRAFFIC_JAM_TIME = "TRAFFIC_JAM_TIME";
        public const string CLEAN_ALL = "CLEAN_ALL";
        public const string BOOKING_TIME_FRAME = "BOOKING_TIME_FRAME";
        public const string INTERVAL_TIME_FRAME = "INTERVAL_TIME_FRAME";
        public const string CALCULATE_CREDIT = "CALCULATE_CREDIT";

        public const string MAX_DISTANCE = "MAX_DISTANCE";
        public const string ESTIMATE_NORMAL_TIME = "ESTIMATE_NORMAL_TIME";
        public const string ESTIMATE_TRAFFIC_JAM_TIME = "ESTIMATE_TRAFFIC_JAM_TIME";
        public const string DISTRIBUTE_CLEANING_TOOL = "DISTRIBUTE_CLEANING_TOOL";
        public const string PENDING_MINUTES_CONFIRM_BOOKING = "PENDING_MINUTES_CONFIRM_BOOKING";
        public const string USE_COMPANY_TOOL = "USE_COMPANY_TOOL";
        public const string NUMBER_RE_CLEAN = "NUMBER_RE_CLEAN";
        public const string INTERVAL_MIN_LENGHT = "INTERVAL_MIN_LENGHT";
        public const string EMPLOYEE_CODE_LENGHT = "EMPLOYEE_CODE_LENGHT";
        public const string BAN_REJECT = "BAN_REJECT";
    }

    // BOOKING - FEEDBACK - DEPOSIT
    public class ConstNotificationType
    {
        public const string BOOKING = "BOOKING";
        public const string FEEDBACK = "FEEDBACK";
        public const string DEPOSIT = "DEPOSIT";
        public const string REQUEST = "REQUEST";
    }

    // NORMAL, OPTIONAL, OVERALL
    public class ConstServiceGroupType
    {
        public const string NORMAL = "NORMAL";
        public const string OPTIONAL = "OPTIONAL";
        public const string OVERALL = "OVERALL";
    }
    
    // NORMAL, OPTIONAL, OVERALL
    public class ConstServiceType
    {
        public const string QUANTITY = "QUANTITY";
        public const string AREA = "AREA";
    }

    // BOOKING - USER - COMPANY
    public class ConstTransactionType
    {
        public const string USER = "USER";
        //public const string BOOKING = "BOOKING";
        public const string COMPANY = "COMPANY";
    }

    //  PENDING - APPROVED - PROVIDED - REJECTED - CANCELLED
    public class ConstRequestStatus
    {
        public const string PENDING = "PENDING";
        public const string APPROVED = "APPROVED";
        public const string PROVIDED = "PROVIDED";
        public const string REJECTED = "REJECTED";
        public const string CANCELLED = "CANCELLED";
    }
    #endregion

    #region Order Constans
    public class ConstActiveCode
    {
        public const string SUCCESS = "SUCCESS";
        public const string EXPIRED = "EXPIRED";
        public const string WRONG_CODE = "WRONG_CODE";
    }
    public class ConstFirebase
    {
        public const string ApiKey = "AIzaSyDUS-mp2WdeAAMgLYkz4PL54hFY7Md0ApY";
        public const string Bucket = "testimage-123.appspot.com";
        public const string AuthEmail = "test@gmail.com";
        public const string AuthPassword = "123456";
    }

    public class ConstGeneral
    {
        public const string COMPANY_USERNAME = "beclean.co";
    }
    #endregion

}
