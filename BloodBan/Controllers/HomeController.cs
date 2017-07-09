using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BloodBan.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
namespace BloodBan.Controllers
{
    public class HomeController : Controller
    {
       bloodbankEntities9 db = new bloodbankEntities9();
        public ActionResult Index()
        {
            ViewBag.Message = "Index page.";
            
            return View();
        }

        public ActionResult signup()
        {
            
            ViewBag.Message = "Your signup page.";

            return View();
        }
        public ActionResult Login()
        {
            string email = Request["userName"];
            string password = Request["password"];
            try
            {
                user us = db.users.First(u => u.email == email && u.password == password);
                
                Session["name"] = us.firstname +" " +us.lastname;
                Session["id"] = us.Id;
                Session["email"] =us. email;
                Session["bloodGroup"] = us.bloodgroup;
                Session["city"] = us.city;
                return Redirect("userHome");
                
            }
            catch (Exception e)
            {
                return Redirect("Index");
            }
            // select from db 
        }
        public ActionResult Logout()
        {
            
            Session.RemoveAll();
            return Redirect("Index");
        }

      
        public ActionResult signupvalidation()
        {
            
                string fname = Request["firstName"];
                string lname = Request["lastName"];
                string email = Request["email"];
                string password = Request["pass"];
                string phoneNo = Request["countrycode"]+Request["phone"];
                DateTime dob = Convert.ToDateTime(Request["dob"]);
                //int m=Convert.ToInt32( Request["bMonth"]);
                //int d = Convert.ToInt32(Request["bDay"]);
                //int y = Convert.ToInt32(Request["bYear"]);
                //DateTime dob = new DateTime(y, m, d,0,0,0);
                string gender = Request["gender"];
                string city = Request["city"];
                string bloodgroup =Request["blood group"];
                String isagree = Request["isAgree"];    
            //int agreed =Convert.ToInt32((Request["is agree"]));
                //Session["agreed"] = Request["is agree"];
            user u = new user();
                u.firstname = fname;
                u.lastname = lname;
                u.email = email;
                u.password = password;
                u.phoneNo = phoneNo;
                u.DOB = dob;
                u.gender =gender;
                u.city = city;
                u.bloodgroup = bloodgroup;
                
            
                if (isagree == "on")
                {
                    u.agreed = true;
                }
                db.users.Add(u);
                db.SaveChanges();
                Session["name"] = u.firstname + " " + u.lastname;
                Session["id"] = u.Id;
                Session["email"] = u.email;
                Session["bloodGroup"] = u.bloodgroup;
                Session["city"] = u.city;

            // Maintain sessions same as in Login function
                return Redirect("userHome");

           
        }
        public ActionResult bloodrequests()
        {
            if (Session["id"] != null)
            {

                ViewBag.Message = "bloodrequest page.";
                string bloodGroup = (string)Session["bloodGroup"];
                string city = (string)Session["city"];

                List<bloodrequest> reqs = db.bloodrequests.Where(r => r.bloodgroup == bloodGroup && r.presentlocation == city && r.ishandle == false).ToList();

                ViewBag.size = reqs.Count;
                return View(reqs); // pass that list from this function
            }
            else
            {
                return View("Index");
            }
        }
        public ActionResult Addrequest()
        {
            ViewBag.Message = "Add Request.";

            string bloodGroup = Request["bloodGroup"];
            string city = Request["city"];
            int id = (int)Session["id"];
          
            bloodrequest br = new bloodrequest();
            br.userid = id;
            br.bloodgroup = bloodGroup;
            br.presentlocation = city;
            br.datetime = System.DateTime.Now;
          
            // Add this request to blood request table
            db.bloodrequests.Add(br);
            db.SaveChanges();

            user requester = db.users.Find(id);

            List<user> users = db.users.Where(u => u.bloodgroup == bloodGroup && u.city == city).ToList();
            List<String>  emails= new List<String>();
            foreach (user u in users)
            {
                if (u.donations.Count == 0)
                {
                    emails.Add(u.email);
                }
               else if ( (DateTime.Now - u.donations.OrderByDescending(u1 => u1.datetime).First().datetime).TotalDays > 60)
                {
                    emails.Add(u.email);
                }
           }
            

            sendEmail(emails, "Pakistan Blood Bank",bloodGroup + " Blood is urgently required in " + city +" " +"Kindly contact :"+requester.phoneNo);

            //////
            ViewBag.Message = "bloodrequest page.";
            string bloodGroup1 = (string)Session["bloodGroup"];
            string city1 = (string)Session["city"];

            List<bloodrequest> reqs = db.bloodrequests.Where(r => r.bloodgroup == bloodGroup1 && r.presentlocation == city1 && r.ishandle == false).ToList();
            ViewBag.size = reqs.Count;
            return RedirectToAction("bloodrequests",reqs);
        }

        public void sendEmail(List<String> tos, String subject, String body)
        {
            try
            {
                // message.To.Add(email);
                var message = new MailMessage();
                foreach (String to in tos)
                {
                    message.To.Add(new MailAddress(to));
                }
                
                
                // replace with valid value 
                message.From = new MailAddress("pakistanbloodbanksystem@gmail.com");  // replace with valid value
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                
                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "pakistanbloodbanksystem@gmail.com",  // replace with valid value
                        Password = "bloodbank123"  // replace with valid value
                    };
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                    /////////////////////

                }
            }



            catch (Exception e) { }

        }
        public ActionResult AddDonation()
        {
            ViewBag.Message = "Add Donation.";
            int userid = (int)Session["id"];
            int requestid =int.Parse(Request["reqID"]);

            // check 2 months.
            try
            {
                donation don = db.donations.Where(d => d.userid == userid).OrderByDescending(m => m.datetime).First();
                double days = (DateTime.Now - don.datetime).TotalDays;
                if (days > 60)
                {
                    bloodrequest req = db.bloodrequests.Find(requestid);
                    req.ishandle = true;
                    db.SaveChanges();
                    donation d = new donation();
                    d.bloodrequestid = requestid;
                    d.userid = userid;
                    d.datetime = DateTime.Now;
                    db.donations.Add(d);
                    db.SaveChanges();

                }
            }
            catch (Exception e) {
                bloodrequest req = db.bloodrequests.Find(requestid);
                req.ishandle = true;
               // db.SaveChanges();
                donation d = new donation();
                d.bloodrequestid = requestid;
                d.userid = userid;
                d.datetime = DateTime.Now;
                db.donations.Add(d);
                db.SaveChanges();
            }
            
            // Add donation here

            return RedirectToAction("bloodrequests");
        }
        public ActionResult Addpost()
        {
            ViewBag.Message = "Share Experience.";
            //string uH = Request["userHome"];
            string post = Request["postdata"];
            experience e = new experience();
            e.experimenttext = post;
            e.DateTime = DateTime.Now;
            //user us = (user)db.users.First(u => u.email == Session["email"]);
            e.userid = (int)Session["id"];

            db.experiences.Add(e);
            db.SaveChanges();
            return RedirectToAction("userHome");
        }
        public ActionResult bloodfactors()
        {
            ViewBag.Message = "blood factors.";

            return View();
        }
        public ActionResult bloodtips()
        {
            ViewBag.Message = "blood tips.";

            return View();
        }

        public ActionResult userHome()
        {
            if (Session["id"] != null)
            {
                ViewBag.Message = "user Home.";

                // Get list ofuid=iences.OrderByDescending(p => p.DateTime).ToList();

                List<experience> list = new List<experience>();
                list = db.experiences.OrderByDescending(p => p.DateTime).ToList();

                return View(list);
            }
            else {
                return RedirectToAction("Index");
            }
        }

        public String DoLike(int exId)
        {
            
            
            int uid = (int)Session["id"];

            try
            {
                like lk = db.likes.First(l =>l.expid == exId && l.userid == uid);
                db.likes.Remove(lk);
                db.SaveChanges();

            }
            catch(Exception ex)
            {
                like l = new like();
                l.userid = uid;
                l.expid = exId;
                db.likes.Add(l);
                db.SaveChanges();
            }

            experience e = db.experiences.Find(exId);
            return e.likes.Count + "";
            //get user id from Session
            // check either this person already likes this post.
            // Exceptio ===> make like object, set values, Add to db, save changes.
        
             }
             
        public ActionResult searchedoner()
        {
            ViewBag.Message = "Search Blood.";
            string bg = Request["bloodgroup"];
            string city = Request["city"];
            List<user> users = db.users.Where(u => u.bloodgroup == bg && u.city == city).ToList();
            ViewBag.size = users.Count;
            Session["donerBG"] =bg+" ";
            Session["donerCity"] = city+" ";
            Session.Remove("Search Blood");
            return View(users);
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Slider()
        {
            return View();
        }
    }
}