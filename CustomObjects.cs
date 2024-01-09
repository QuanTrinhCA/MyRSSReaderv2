using System;
using System.Collections.Generic;

namespace CustomObjects
{
    public class CustomFeed
    {
        public string Description;
        public string ImageLink;
        public string Link;
        public string Title;
    }

    public class CustomFeedEqualityComparer : EqualityComparer<CustomFeed>
    {
        public override bool Equals(CustomFeed customFeed1, CustomFeed customFeed2)
        {
            if (customFeed1 == null && customFeed2 == null)
            {
                return true;
            }
            else if (customFeed1 == null || customFeed2 == null)
            {
                return false;
            }
            return (customFeed1.Title == customFeed2.Title) && (customFeed1.Link == customFeed2.Link);
        }

        public override int GetHashCode(CustomFeed customFeed)
        {
            string hCode = customFeed.Title + customFeed.Link;
            return hCode.GetHashCode();
        }
    }

    public class CustomFeedItem
    {
        public ICollection<string> Categories;
        public string ImageLink;
        public string Link;
        public string Publisher;
        public DateTime PublishingDate;
        public string PublishingDateString;
        public string Title;
    }

    public class CustomArticleItem
    {
        public string Title;
        public string Link;
        public string HtmlContent;
    }
}