﻿namespace PassKitHelper
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public class PassInfoBuilder
    {
        public const string MimeContentType = "application/json";

        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
            NullValueHandling = NullValueHandling.Ignore,
            DateParseHandling = DateParseHandling.DateTimeOffset,
        };

        private readonly IDictionary<string, object> values;

        public PassInfoBuilder()
        {
            values = new Dictionary<string, object>();
        }

        protected PassInfoBuilder(PassInfoBuilder parent)
        {
            values = parent.values;
        }

        public StandardBuilder Standard => new StandardBuilder(this);

        public AssociatedAppBuilder AssociatedApp => new AssociatedAppBuilder(this);

        public CompanionAppBuilder CompanionApp => new CompanionAppBuilder(this);

        public ExpirationBuilder ExpirationKeys => new ExpirationBuilder(this);

        public RelevanceBuilder Relevance => new RelevanceBuilder(this);

        public StyleBuilder BoardingPass => new StyleBuilder(nameof(BoardingPass), this);

        public StyleBuilder Coupon => new StyleBuilder(nameof(Coupon), this);

        public StyleBuilder EventTicket => new StyleBuilder(nameof(EventTicket), this);

        public StyleBuilder Generic => new StyleBuilder(nameof(Generic), this);

        public StyleBuilder StoreCard => new StyleBuilder(nameof(StoreCard), this);

        public VisualAppearanceBuilder VisualAppearance => new VisualAppearanceBuilder(this);

        public WebServiceBuilder WebService => new WebServiceBuilder(this);

        public static string GetCaller([CallerMemberName] string caller = "unknown")
        {
            return caller;
        }

        public void SetValue(string name, PassInfoBuilder value)
        {
            values[name.ToCamelCase()] = value.values;
        }

        public void SetValue(string name, object value)
        {
            values[name.ToCamelCase()] = value;
        }

        public void AppendValue(string name, object value)
        {
            if (!values.TryGetValue(name.ToCamelCase(), out var list))
            {
                list = new List<object>();
                values[name.ToCamelCase()] = list;
            }

            ((List<object>)list).Add(value);
        }

        public JObject Build()
        {
            return JObject.FromObject(values, JsonSerializer.Create(JsonSettings));
        }

        public class StandardBuilder : PassInfoBuilder
        {
            public StandardBuilder(PassInfoBuilder parent)
                : base(parent)
            {
                // Required. Version of the file format. The value must be 1.
                SetValue("FormatVersion", 1);
            }
        }

        public class AssociatedAppBuilder : PassInfoBuilder
        {
            public AssociatedAppBuilder(PassInfoBuilder parent)
                : base(parent)
            {
            }
        }

        public class CompanionAppBuilder : PassInfoBuilder
        {
            public CompanionAppBuilder(PassInfoBuilder parent)
                : base(parent)
            {
            }
        }

        public class ExpirationBuilder : PassInfoBuilder
        {
            public ExpirationBuilder(PassInfoBuilder parent)
                : base(parent)
            {
            }
        }

        public class RelevanceBuilder : PassInfoBuilder
        {
            public RelevanceBuilder(PassInfoBuilder parent)
                : base(parent)
            {
            }
        }

        public class StyleBuilder : PassInfoBuilder
        {
            private readonly Dictionary<string, object> styleValues;

            public StyleBuilder(string style, PassInfoBuilder parent)
                : base(parent)
            {
                styleValues = new Dictionary<string, object>();
                parent.SetValue(style, styleValues);
            }

            protected StyleBuilder(StyleBuilder parent)
                : base(parent)
            {
                styleValues = parent.styleValues;
            }

            public StandardFieldsBuilder AuxiliaryFields => new StandardFieldsBuilder(nameof(AuxiliaryFields), this);

            public StandardFieldsBuilder BackFields => new StandardFieldsBuilder(nameof(BackFields), this);

            public StandardFieldsBuilder HeaderFields => new StandardFieldsBuilder(nameof(HeaderFields), this);

            public StandardFieldsBuilder PrimaryFields => new StandardFieldsBuilder(nameof(PrimaryFields), this);

            public StandardFieldsBuilder SecondaryFields => new StandardFieldsBuilder(nameof(SecondaryFields), this);

            public void SetStyleValue(string name, object value)
            {
                styleValues[name.ToCamelCase()] = value;
            }

            public void AppendStyleValue(string name, object value)
            {
                if (!styleValues.TryGetValue(name.ToCamelCase(), out var list))
                {
                    list = new List<object>();
                    styleValues[name.ToCamelCase()] = list;
                }

                ((List<object>)list).Add(value);
            }
        }

        public class StandardFieldsBuilder : StyleBuilder
        {
            private readonly string collectionName;

            public StandardFieldsBuilder(string collectionName, StyleBuilder parent)
                : base(parent)
            {
                this.collectionName = collectionName;
            }

            public StandardFieldsBuilder(StandardFieldsBuilder parent)
                : base(parent)
            {
                this.collectionName = parent.collectionName;
            }

            public StandardFieldBuilder Add(string key) => new StandardFieldBuilder(collectionName, key, this);
        }

        public class StandardFieldBuilder : StandardFieldsBuilder
        {
            private readonly Dictionary<string, object> fieldValues;

            public StandardFieldBuilder(string collectionName, string key, StandardFieldsBuilder parent)
                : base(parent)
            {
                fieldValues = new Dictionary<string, object>();
                parent.AppendStyleValue(collectionName, fieldValues);
                SetFieldValue(nameof(key), key);
            }

            public void SetFieldValue(string name, object value)
            {
                fieldValues[name.ToCamelCase()] = value;
            }
        }

        public class VisualAppearanceBuilder : PassInfoBuilder
        {
            public VisualAppearanceBuilder(PassInfoBuilder parent)
                : base(parent)
            {
            }
        }

        public class WebServiceBuilder : PassInfoBuilder
        {
            public WebServiceBuilder(PassInfoBuilder parent)
                : base(parent)
            {
            }
        }

        public class PassInfoBuilderNfcKeys : PassInfoBuilder
        {
            public PassInfoBuilderNfcKeys(PassInfoBuilder parent)
                : base(parent)
            {
            }
        }

        public class Beacon
        {
            public Beacon(string proximityUUID)
            {
                this.ProximityUUID = proximityUUID;
            }

            public string ProximityUUID { get; set; }

            public uint? Major { get; set; }

            public uint? Minor { get; set; }

            public string? RelevantText { get; set; }
        }

        public class Location
        {
            public double Latitude { get; set; }

            public double Longitude { get; set; }

            public double? Altitude { get; set; }

            public string? RelevantText { get; set; }
        }

        public class Barcode
        {
            public Barcode(string message, BarcodeFormat format)
            {
                this.Message = message;
                this.Format = format;
            }

            [JsonIgnore]
            public BarcodeFormat Format { get; set; }

            [JsonProperty("format")]
            public string FormatAsString
            {
                get => Format.ToPassKitString();
            }

            public string Message { get; set; }

            public string? AltText { get; set; }

            public string MessageEncoding { get; set; } = "iso-8859-1";
        }

        public class Nfc
        {
            public Nfc(string message)
            {
                this.Message = message;
            }

            public string Message { get; set; }

            public string? EncryptionPublicKey { get; set; }
        }
    }
}