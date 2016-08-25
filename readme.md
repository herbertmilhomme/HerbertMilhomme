
<!-- saved from url=(0095)http://programmers.stackexchange.com/revisions/040cdfd2-4b9d-4d72-b8c9-c631ccc01db5/view-source -->
<html><head><meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title>Revision 040cdfd2-4b9d-4d72-b8c9-c631ccc01db5 - Programmers Stack Exchange</title>
</head>
<body>
<pre style="width:650px; white-space:pre-wrap">The idea here is that most of us should _already_ know _most_ of what is on this list.  But there just might be one or two items you haven't really looked into before, don't fully understand, or maybe never even heard of.

**Interface and User Experience**

 - Be  aware that browsers implement standards inconsistently and make sure your site works reasonably well across all major browsers.  At a minimum test against a recent [Gecko][1] engine ([Firefox][2]), a WebKit engine ([Safari][3] and some mobile browsers), [Chrome][4], your supported [IE browsers][5] (take advantage of the [Application Compatibility VPC Images][6]), and [Opera][7]. Also consider how [browsers render your site][8] in different operating systems.
 - Consider how people might use the site other than from the major browsers: cell phones, screen readers and search engines, for example. &amp;mdash; Some accessibility info: [WAI][9] and [Section508][10], Mobile development: [MobiForge][11].
 - Staging: How to deploy updates without affecting your users.  Have one or more test or staging environments available to implement changes to architecture, code or sweeping content and ensure that they can be deployed in a controlled way without breaking anything. Have an automated way of then deploying approved changes to the live site. This is most effectively implemented in conjunction with the use of a version control system (CVS, Subversion, etc.) and an automated build mechanism (Ant, NAnt, etc.).
 - Don't display unfriendly errors directly to the user.
 - Don't put users' email addresses in plain text as they will get spammed to death.
 - Add the attribute `rel="nofollow"` to user-generated links [to avoid spam][12].
 - [Build well-considered limits into your site][13] - This also belongs under Security.
 - Learn how to do [progressive enhancement][14].
 - [Redirect after a POST][15] if that POST was successful, to prevent a refresh from submitting again.
 - Don't forget to take accessibility into account.  It's always a good idea and in certain circumstances it's a [legal requirement][16].  [WAI-ARIA][17] and [WCAG 2][18] are good resources in this area.
 - [Don't make me think][19]

**Security**

  - It's a lot to digest but the [OWASP development guide][20] covers Web Site security from top to bottom.
  - Know about Injection especially [SQL injection][21] and how to prevent it.
  - Never trust user input, nor anything else that comes in the request (which includes cookies and hidden form field values!).
  - Hash passwords using [salt][22] and use different salts for your rows to prevent rainbow attacks. Use a slow hashing algorithm, such as bcrypt (time tested) or scrypt (even stronger, but newer) ([1][23], [2][24]), for storing passwords. ([How To Safely Store A Password][25]). The [NIST also approves of PBKDF2 to hash passwords][26]", and it's [FIPS approved in .NET][27] (more info [here][28]). *Avoid* using MD5 or SHA family directly.
  - [Don't try to come up with your own fancy authentication system](http://stackoverflow.com/questions/1581610/how-can-i-store-my-users-passwords-safely/1581919#1581919). It's such an easy thing to get wrong in subtle and untestable ways and you wouldn't even know it until _after_ you're hacked.
  - Know the [rules for processing credit cards][29]. ([See this question as well][30])
  - Use [SSL][31]/[HTTPS][32] for login and any pages where sensitive data is entered (like credit card info).
  - [Prevent session hijacking][33].
  - Avoid [cross site scripting][34] (XSS).
  - Avoid [cross site request forgeries][35] (CSRF).
  - Avoid [Clickjacking][36].
  - Keep your system(s) up to date with the latest patches.
  - Make sure your database connection information is secured.
  - Keep yourself informed about the latest attack techniques and vulnerabilities affecting your platform.
  - Read [The Google Browser Security Handbook][37].
  - Read [The Web Application Hacker's Handbook][38].
  - Consider [The principle of least privilege][39]. Try to run your app server [as non-root][40]. ([tomcat example][41])

**Performance**

  - Implement caching if necessary, understand and use [HTTP caching][42] properly as well as [HTML5 Manifest][43].
  - Optimize images - don't use a 20 KB image for a repeating background.
  - Learn how to [gzip/deflate content][44] (&lt;strike&gt;[deflate is better][45]&lt;/strike&gt;).
  - Combine/concatenate multiple stylesheets or multiple script files to reduce number of browser connections and improve gzip ability to compress duplications between files.
  - Take a look at the [Yahoo Exceptional Performance][46] site, lots of great guidelines, including improving front-end performance and their [YSlow][47] tool (requires Firefox, Safari, Chrome or Opera). Also, [Google page speed][48] (use with [browser extension][49]) is another tool for performance profiling, and it optimizes your images too.
  - &lt;strike&gt;Use [CSS Image Sprites][50] for small related images like toolbars (see the "minimize HTTP requests" point)&lt;/strike&gt;
  - Use [SVG image sprites](https://24ways.org/2014/an-overview-of-svg-sprite-creation-techniques/) for small related images like toolbars. SVG coloring is bit tricky. You can read about it [here](http://tympanus.net/codrops/2015/07/16/styling-svg-use-content-css/).
  - Busy web sites should consider [splitting components across domains][51].  Specifically...
  - Static content (i.e. images, CSS, JavaScript, and generally content that doesn't need access to cookies) should go in a separate domain _[that does not use cookies][52]_, because all cookies for a domain and its subdomains are sent with every request to the domain and its subdomains.  One good option here is to use a Content Delivery Network (CDN), but consider the case where that CDN may fail by including alternative CDNs, or local copies that can be served instead.
  - Minimize the total number of HTTP requests required for a browser to render the page.
  - &lt;strike&gt;Utilize [Google Closure Compiler][53] for JavaScript and [other minification tools][54].&lt;/strike&gt; 
  - Choose a [template engine](http://garann.github.io/template-chooser/) and render/pre-compile it using task-runners like gulp or grunt
  - Make sure there’s a `favicon.ico` file in the root of the site, i.e. `/favicon.ico`. [Browsers will automatically request it](http://mathiasbynens.be/notes/rel-shortcut-icon), even if the icon isn’t mentioned in the HTML at all. If you don’t have a `/favicon.ico`, this will result in a lot of 404s, draining your server’s bandwidth.

**SEO (Search Engine Optimization)**

  - Use "search engine friendly" URLs, i.e. use `example.com/pages/45-article-title` instead of `example.com/index.php?page=45`
  - When using `#` for dynamic content change the `#` to `#!` and then on the server `$_REQUEST["_escaped_fragment_"]` is what googlebot uses instead of `#!`. In other words, `./#!page=1` becomes `./?_escaped_fragments_=page=1`. Also, for users that may be using FF.b4 or Chromium, `history.pushState({"foo":"bar"}, "About", "./?page=1");` Is a great command. So even though the address bar has changed the page does not reload. This allows you to use `?` instead of `#!` to keep dynamic content and also tell the server when you email the link that we are after this page, and the AJAX does not need to make another extra request.
  - Don't use links that say ["click here"][55]. You're wasting an SEO opportunity and it makes things harder for people with screen readers.
  - Have an [XML sitemap][56], preferably in the default location `/sitemap.xml`.
  - Use [`&lt;link rel="canonical" ... /&gt;`][57] when you have multiple URLs that point to the same content, this issue can also be addressed from [Google Webmaster Tools][58].
  - Use [Google Webmaster Tools][59] and [Bing Webmaster Tools][60].
  - Install [Google Analytics][61] right at the start (or an open source analysis tool like [Piwik][62]).
  - Know how [robots.txt][63] and search engine spiders work.
  - Redirect requests (using `301 Moved Permanently`) asking for `www.example.com` to `example.com` (or the other way round) to prevent splitting  the google ranking between both sites.
  - Know that there can be badly-behaved spiders out there.
  - If you have non-text content look into Google's sitemap extensions for video etc. There is some good information about this in [Tim Farley's answer][64].

**Technology**

   - Understand [HTTP][65] and things like GET, POST, sessions, cookies, and what it means to be "stateless".
   - Write your [XHTML][66]/[HTML][67] and [CSS][68] according to the [W3C specifications][69] and make sure they [validate][70].  The goal here is to avoid browser quirks modes and as a bonus make it much easier to work with non-traditional browsers like screen readers and mobile devices.
   - Understand how JavaScript is processed in the browser.
   - Understand how JavaScript, style sheets, and other resources used by your page are loaded and consider their impact on *perceived* performance. It is now widely regarded as appropriate to [move scripts to the bottom][71] of your pages with exceptions typically being things like analytics apps or HTML5 shims.
   - Understand how the JavaScript sandbox works, especially if you intend to use iframes.
   - Be aware that JavaScript can and will be disabled, and that AJAX is therefore an extension, not a baseline.  Even if most normal users leave it on now, remember that [NoScript][72] is becoming more popular, mobile devices may not work as expected, and Google won't run most of your JavaScript when indexing the site.
   - Learn the [difference between 301 and 302 redirects][73] (this is also an SEO issue).
   - Learn as much as you possibly can about your deployment platform.
   - Consider using a [Reset Style Sheet][74] or [normalize.css][75].
   - Consider JavaScript frameworks (such as [jQuery][76], [MooTools][77], [Prototype][78], [Dojo][79] or [YUI 3][80]), which will hide a lot of the browser differences when using JavaScript for DOM manipulation.
   - Taking perceived performance and JS frameworks together, consider using a service such as the [Google Libraries API][81] to load frameworks so that a browser can use a copy of the framework it has already cached rather than downloading a duplicate copy from your site.
   - Don't reinvent the wheel. Before doing ANYTHING search for a component or example on how to do it. There is a 99% chance that someone has done it and released an OSS version of the code.
   - On the flipside of that, don't start with 20 libraries before you've even decided what your needs are. Particularly on the client-side web where it's almost always ultimately more important to keep things lightweight, fast, and flexible.

**Bug fixing**

  - Understand you'll spend 20% of your time coding and 80% of it maintaining, so code accordingly.
  - Set up a good error reporting solution.
  - Have a system for people to contact you with suggestions and criticisms.
  - Document how the application works for future support staff and people performing maintenance.
  - Make frequent backups! (And make sure those backups are functional) Have a restore strategy, not just a backup strategy.
  - Use a version control system to store your files, such as [Subversion][82], [Mercurial][83] or [Git][84].
  - Don't forget to do your Acceptance Testing.  Frameworks like [Selenium][85] can help. Especially if you fully automate your testing, perhaps by using a Continuous Integration tool, such as [Jenkins][86].
  - Make sure you have sufficient logging in place using frameworks such as [log4j][87], [log4net][88] or [log4r][89]. If something goes wrong on your live site, you'll need a way of finding out what.
  - When logging make sure you capture both handled exceptions, and unhandled exceptions. Report/analyse the log output, as it'll show you where the key issues are in your site.



**Other**

  - Implement both server-side and client-side monitoring and analytics (one should be proactive rather than reactive).
  - Use services like UserVoice and Intercom (or any other similar tools) to constantly keep in touch with your users.
  - Follow [Vincent Driessen][90]'s [Git branching model][91]


Lots of stuff omitted not necessarily because they're not useful answers, but because they're either too detailed, out of scope, or go a bit too far for someone looking to get an overview of the things they should know. Please feel free to edit this as well, I probably missed some stuff or made some mistakes.

  [1]: https://en.wikipedia.org/wiki/Gecko_%28layout_engine%29
  [2]: http://firefox.com/
  [3]: http://www.apple.com/safari/
  [4]: http://www.google.com/chrome
  [5]: https://en.wikipedia.org/wiki/Internet_Explorer
  [6]: http://www.microsoft.com/Downloads/details.aspx?FamilyID=21eabb90-958f-4b64-b5f1-73d0a413c8ef&amp;displaylang=en
  [7]: http://www.opera.com/
  [8]: http://www.browsershots.org
  [9]: http://www.w3.org/WAI/
  [10]: http://www.section508.gov/
  [11]: http://mobiforge.com/
  [12]: https://en.wikipedia.org/wiki/Nofollow
  [13]: http://www.codinghorror.com/blog/archives/001228.html
  [14]: https://en.wikipedia.org/wiki/Progressive_enhancement
  [15]: https://en.wikipedia.org/wiki/Post/Redirect/Get
  [16]: http://www.section508.gov/
  [17]: http://www.w3.org/WAI/intro/aria
  [18]: http://www.w3.org/TR/WCAG20/
  [19]: http://www.sensible.com/dmmt.html
  [20]: http://www.owasp.org/index.php/Category:OWASP_Guide_Project
  [21]: https://en.wikipedia.org/wiki/SQL_injection
  [22]: https://security.stackexchange.com/q/21263/396
  [23]: http://www.tarsnap.com/scrypt.html
  [24]: http://it.slashdot.org/comments.pl?sid=1987632&amp;cid=35149842
  [25]: http://codahale.com/how-to-safely-store-a-password/
  [26]: https://security.stackexchange.com/q/7689/396
  [27]: https://security.stackexchange.com/a/2136/396
  [28]: https://security.stackexchange.com/questions/211/how-to-securely-hash-passwords
  [29]: https://www.pcisecuritystandards.org/
  [30]: https://stackoverflow.com/questions/51094/payment-processors-what-do-i-need-to-know-if-i-want-to-accept-credit-cards-on-m
  [31]: http://www.mozilla.org/projects/security/pki/nss/ssl/draft302.txt
  [32]: https://en.wikipedia.org/wiki/Https
  [33]: https://en.wikipedia.org/wiki/Session_hijacking#Prevention
  [34]: https://en.wikipedia.org/wiki/Cross-site_scripting
  [35]: https://en.wikipedia.org/wiki/Cross-site_request_forgery
  [36]: https://en.wikipedia.org/wiki/Clickjacking
  [37]: https://code.google.com/archive/p/browsersec/
  [38]: http://amzn.com/0470170778
  [39]: https://en.wikipedia.org/wiki/Principle_of_least_privilege
  [40]: https://security.stackexchange.com/questions/47576/do-simple-linux-servers-really-need-a-non-root-user-for-security-reasons
  [41]: http://tomcat.apache.org/tomcat-8.0-doc/security-howto.html#Non-Tomcat_settings
  [42]: http://www.mnot.net/cache_docs/
  [43]: http://www.w3.org/TR/2011/WD-html5-20110525/offline.html
  [44]: http://developer.yahoo.com/performance/rules.html#gzip "gzip content"
  [45]: https://stackoverflow.com/questions/1574168/gzip-vs-deflate-zlib-revisited
  [46]: http://developer.yahoo.com/performance/
  [47]: http://developer.yahoo.com/yslow/
  [48]: https://developers.google.com/speed/docs/best-practices/rules_intro
  [49]: https://developers.google.com/speed/pagespeed/insights_extensions
  [50]: http://alistapart.com/articles/sprites
  [51]: http://developer.yahoo.com/performance/rules.html#split
  [52]: http://blog.stackoverflow.com/2009/08/a-few-speed-improvements/
  [53]: http://developers.google.com/closure/compiler/
  [54]: http://developer.yahoo.com/yui/compressor/
  [55]: https://ux.stackexchange.com/questions/12100/why-shouldnt-we-use-the-word-here-in-a-textlink
  [56]: http://www.sitemaps.org/
  [57]: http://googlewebmastercentral.blogspot.com/2009/02/specify-your-canonical.html
  [58]: http://www.google.com/webmasters/
  [59]: http://www.google.com/webmasters/
  [60]: http://www.bing.com/toolbox/webmaster
  [61]: http://www.google.com/analytics/
  [62]: http://piwik.org/
  [63]: https://en.wikipedia.org/wiki/Robots_exclusion_standard
  [64]: http://stackoverflow.com/questions/72394/what-should-a-developer-know-before-building-a-public-web-site#167608
  [65]: http://www.ietf.org/rfc/rfc2616.txt
  [66]: http://www.w3.org/TR/xhtml1/
  [67]: http://www.w3.org/TR/REC-html40/
  [68]: http://www.w3.org/TR/CSS2/
  [69]: http://www.w3.org/TR/
  [70]: http://validator.w3.org/
  [71]: https://developer.yahoo.com/blogs/ydn/high-performance-sites-rule-6-move-scripts-bottom-7200.html
  [72]: http://noscript.net/
  [73]: http://www.bigoakinc.com/blog/when-to-use-a-301-vs-302-redirect/
  [74]: http://stackoverflow.com/questions/167531/is-it-ok-to-use-a-css-reset-stylesheet
  [75]: http://necolas.github.com/normalize.css/
  [76]: http://jquery.com/
  [77]: http://mootools.net/
  [78]: http://www.prototypejs.org/
  [79]: http://dojotoolkit.org
  [80]: http://developer.yahoo.com/yui/3/
  [81]: http://developers.google.com/speed/libraries/devguide
  [82]: http://subversion.apache.org/
  [83]: http://mercurial.selenic.com/
  [84]: http://git-scm.org
  [85]: http://seleniumhq.org/
  [86]: http://jenkins-ci.org/
  [87]: http://logging.apache.org/log4j/
  [88]: http://logging.apache.org/log4net/
  [89]: http://log4r.rubyforge.org/
  [90]: http://nvie.com/about/
  [91]: http://nvie.com/posts/a-successful-git-branching-model/
</pre>

</body></html>