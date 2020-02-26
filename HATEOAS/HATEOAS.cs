using System.Collections.Generic;

namespace API.HATEOAS
{
    public class HATEOAS
    {
        public string url;
        public string protocol = "https://";
        public List<Link> actions = new List<Link>();

        public HATEOAS (string url) {
            this.url = url;
        }

        public HATEOAS (string url, string protocol) {
            this.url = url;
            this.protocol = protocol;
        }

        public void AddAction (string rel, string method) {
            actions.Add(new Link(this.protocol + this.url, rel, method));
        }

        public Link[] GetActions (string id) {
            Link[] tempLinks = new Link[actions.Count];

            for (int i = 0; i < tempLinks.Length; i++) {
                tempLinks[i] = new Link(actions[i].href, actions[i].rel, actions[i].method);
            }


            //montagem do link
            foreach (var link in tempLinks) {
                link.href = link.href + "/" + id.ToString();
            }
            return tempLinks;
        }
    }
}