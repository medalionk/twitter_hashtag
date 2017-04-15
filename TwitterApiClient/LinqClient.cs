using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace TwitterApiClient
{
    /// <summary>
    /// Provides LINQ access to Twitter API via Linq2Twitter.
    /// </summary>
    /// <remarks>
    /// Dependant on web.config appSettings params twitterConsumerKey and twitterConsumerSecret.
    //  Twitter API client oAuth settings: https://dev.twitter.com/app
    /// </remarks>
    public class LinqClient
    {
        readonly int REQUEST_COUNT = 100;
        readonly int PAGE_SIZE = 200;
        readonly string DATE_FORMAT = "ddd, dd MMM yyyy HH:mm:ss";
        readonly int[] TIME_INTERVAL =
            new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12,
                    13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };

        public List<DataPoint> GetTweetsByHour(string query, ulong sinceId)
        {
            List<Status> tweets = new List<Status>();

            var searched = GetTweetsBySearch(query, 0, sinceId, PAGE_SIZE);
            if (searched == null) return new List<DataPoint>();
            
            ulong maxId = searched.SearchMetaData.MaxID;
            tweets.AddRange(searched.Statuses);

            for (int i = 0; i < REQUEST_COUNT; i++)
            {
                searched = GetTweetsBySearch(query, maxId - 1, sinceId, PAGE_SIZE);
                if (searched == null) return new List<DataPoint>();
                maxId = searched.SearchMetaData.MaxID;
                tweets.AddRange(searched.Statuses);
            }

            var groupedTweets = tweets.GroupBy(x =>
                TIME_INTERVAL.FirstOrDefault(r => r > x.CreatedAt.TimeOfDay.TotalHours));

            DataPoint[] result = new DataPoint[TIME_INTERVAL.Count()];
            for (int i = 0; i < TIME_INTERVAL.Count(); i++)
            {
                string dateTime = DateTime.Now.ChangeTime(i, 0, 0).ToString(DATE_FORMAT);
                result[i] = new DataPoint(dateTime, 0);
            }

            foreach (var tweet in groupedTweets)
            {
                int index = tweet.Key - 1;
                result[index] = new DataPoint(result[index].X, tweet.Count());
            }

            return result.ToList();
        }

        private IAuthorizer GetAuthorizer()
        {
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = ConfigurationManager.AppSettings["consumerKey"],
                    ConsumerSecret = ConfigurationManager.AppSettings["consumerSecret"],
                    AccessToken = ConfigurationManager.AppSettings["accessToken"],
                    AccessTokenSecret = ConfigurationManager.AppSettings["accessTokenSecret"]
                }
            };

            return auth;
        }

        private Search GetTweetsBySearch(string query, ulong maxId, ulong sinceId, int pageSize)
        {
            var auth = GetAuthorizer();
            var twitterCtx = new TwitterContext(auth);

            var searchResults =
                from search in twitterCtx.Search
                where search.Type == SearchType.Search &&
                      search.Query == query &&
                      search.Count == pageSize
                select search;

            if (maxId != 0)
            {
                searchResults = searchResults.Where(p => p.MaxID == maxId);
            }

            if (sinceId != 0)
            {
                searchResults = searchResults.Where(p => p.SinceID == sinceId);
            }

            var searched = searchResults.SingleOrDefault();

            if (searched != null)
            {
                return searched;
            }

            return null;
        }
    }
}
