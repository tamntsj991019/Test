using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Util
{
    public class Utils
    {
        public string GenerateRandomCode(int codeLenght)
        {
            // abcdefghijklmnopqrstuvwxyz
            // ABCDEFGHIJKLMNOPQRSTUVWXYZ
            // 0123456789
            const string ranStr = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random rd = new Random();
            string result = "";
            for (int i = 0; i < codeLenght; i++)
            {
                result += ranStr[rd.Next(0, ranStr.Length - 1)];
            }
            return result;
        }

        public string GenerateEmployeeCode(int codeLenght)
        {
            const string ranStr = "0123456789";
            Random rd = new Random();
            string result = "";
            for (int i = 0; i < codeLenght; i++)
            {
                result += ranStr[rd.Next(0, ranStr.Length - 1)];
            }
            return result;
        }

        public bool CheckPointsNear(double checkPointLat, double checkPointLong, double centerPointLat, double centerPointLong, double radius)
        {
            var y = 40000 / 360;
            var x = Math.Cos(Math.PI * centerPointLat / 180.0) * y;
            var dx = Math.Abs(centerPointLong - checkPointLong) * x;//dimension x
            var dy = Math.Abs(centerPointLat - checkPointLat) * y;//dimension y
            return Math.Sqrt(dx * dx + dy * dy) <= radius;
        }

        public double CalculatePointsNear(double checkPointLat, double checkPointLong, double centerPointLat, double centerPointLong)
        {
            var y = 40000 / 360;
            var x = Math.Cos(Math.PI * centerPointLat / 180.0) * y;
            var dx = Math.Abs(centerPointLong - checkPointLong) * x;//dimension x
            var dy = Math.Abs(centerPointLat - checkPointLat) * y;//dimension y
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public TimeSpan ConverStringToTimeSpan(string time)
        {
            string[] times = time.Split(":");
            return new TimeSpan(int.Parse(times[0]), int.Parse(times[1]), 0);
        }

        public double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public bool CheckPointsNearByHaversine(double lat1, double lon1, double lat2, double lon2, double radius)
        {
            const double EARTH_RADIUS = 6371;

            double dlon = ToRadians(lon2 - lon1);
            double dlat = ToRadians(lat2 - lat1);
           
            //double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double a = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Pow(Math.Sin(dlon / 2), 2) * Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (angle * EARTH_RADIUS) <= radius;
        }

        public double CalculatePointsNearByHaversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double EARTH_RADIUS = 6371;

            double dlon = ToRadians(lon2 - lon1);
            double dlat = ToRadians(lat2 - lat1);

            //double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double a = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Pow(Math.Sin(dlon / 2), 2) * Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return angle * EARTH_RADIUS;
        }

        private readonly string[] VietnameseSigns = new string[]
        {
            "aAeEoOuUiIdDyY", 

            "áàạảãâấầậẩẫăắằặẳẵ", "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",

            "éèẹẻẽêếềệểễ", "ÉÈẸẺẼÊẾỀỆỂỄ",

            "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",

            "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ",

            "íìịỉĩ", "ÍÌỊỈĨ", "đ", "Đ", "ýỳỵỷỹ", "ÝỲỴỶỸ"
        };

        public string RepalceSignForVietnameseString(string data)
        {
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    data = data.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return data;
        }

        //#region Parse JSON
        //public List<T> ParseFromJson(T type, string data)
        //{
        //    var settingJson = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.TRAFFIC_JAM_TIME).Description;
        //    // get from DB
        //    return JsonConvert.DeserializeObject<List<TrafficJamTimeModel>>(data);
        //}

        //public string ParseToJson(List<TrafficJamTimeModel> listModel)
        //{
        //    // add to json
        //    return JsonConvert.SerializeObject(listModel);
        //}
        //#endregion
    }
}
