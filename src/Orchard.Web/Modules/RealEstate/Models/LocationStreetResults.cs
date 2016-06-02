using System.Collections.Generic;
using Orchard.Localization;

namespace RealEstate.Models
{
    public class LocationStreetInput
    {
        public string Name { get; set; }
    }

    public class LocationStreetResults
    {
        //Constructors
        public LocationStreetResults(LocationStreetInput input)
        {
            Input = input;
            Valid = true;
        }

        //End Constructors

        //Properties
        public LocationStreetInput Input { get; set; }
        public List<LocationStreetResultMessage> Messages { get; set; }
        public bool Valid { get; set; }
        //End Properties

        //Functions
        public void AddMessage(LocalizedString message)
        {
            AddMessage(message.ToString());
        }

        public void AddMessage(string message)
        {
            AddUserCreationResultMessage(new LocationStreetResultMessage(LocationStreetResultMessageType.Information,
                message));
        }

        public void AddError(LocalizedString error)
        {
            AddError(error.ToString());
        }

        public void AddError(string error)
        {
            if (Valid)
                Valid = false;

            AddUserCreationResultMessage(new LocationStreetResultMessage(LocationStreetResultMessageType.Error, error));
        }

        private void AddUserCreationResultMessage(LocationStreetResultMessage message)
        {
            if (Messages == null)
                Messages = new List<LocationStreetResultMessage>();

            Messages.Add(message);
        }

        //End Functions
    }

    public enum LocationStreetResultMessageType
    {
        Information,
        Warning,
        Error
    }

    public class LocationStreetResultMessage
    {
        public LocationStreetResultMessage(LocationStreetResultMessageType type, string message)
        {
            Type = type;
            Message = message;
        }

        public LocationStreetResultMessageType Type { get; set; }
        public string Message { get; set; }
    }
}