## The purpose of this fork

This fork is intended to explain, in simple terms, that pull request #330 cannot be merged.

I've made every effort to communicate this to the SIT team. Just like i did when we had issues with licences before. However, due to lies regarding myself, King, and MTGA as a whole, the SIT leadership has banned everyone @ MTGA, and refused to listen to our reasoning. And the community's/contributors reasoning has fallen onto death ears. This has left SIT contributors and users misinformed about their legal status to utilize the code granted under the NCSA license, and the entire repository is in violation of paulovs license.

Despite what you may have been told, our goal is what's best for the EmuTarkov community. We hold no hate, or ill-will towards anyone in the community, including anyone currently at SIT. Merging #330 simply isn't the most suitable course of action, and doesnt offer a solution without screwing over your own community.

As I'm blocked from commenting on that PR, I'm forced to create a pull request for explanation.

**The Issue**

When a project combines code with different licenses, it's nesscary to clearly define which license applies to each portion. This is known as license compatibility. If this isn't done, the fault lies not with the license holder, but with the licensee (Paulov). This is precisely the situation we're facing.

![image](https://github.com/EFHDev/StayInTarkov.Client/assets/70953258/0e7fe9ea-e8cf-4a52-8058-4eb017642ae9)

The biggest issue is that, no one knows what fucking code is what. Paulov could of modified someone elses work, or someone could of modified paulovs work. They were not imformed that it was unlicenced work, and i highly doubt you can hold any of this in court without accidently ousting yourself for Fraudulent Misrepresentation as the licence holders had no idea of this crucial detail, and then you suddenly out of the blue revealed it so people are now violators.

This is why you really never see Multi-licence, or even unlicenced work, in software besides closed-source. It creates a mess. If you dont have a good understanding of licences and contract law that is involved with licences, or how to uphold your end of the contract, i really highly encourage you to stick with the preset ones GitHub offers.

More fuckery, so get ready

## Maybe, just maybe, these are possible solutions. 
This is so much of a cluster fuck that you should really get a fucking lawyer involved now.

* **Relinquish Rights:** Paulov could relinquish rights to the code, likely already modified by SIT contributors and others.
* **Revert and License:** Alternatively, the code could be reverted to its original state (before modifications by anyone other than Paulov). Paulov could then add clear license information (File Level Licensing) to any code they wrote (If thats possible, as paulov deleted the last repo and i know for sure Slejm contributed to it.)
* **Honor NCSA License (if Applicable):** Paulov can also choose to honor the NCSA license, provided all contributors agree to void the previous license.

## Why This Matters

The current situation violates the license terms. To put it simply:

This isn't about claiming ownership or personal gain. It's about transparency and upholding the original NCSA agreement, which was presented to everyone who downloaded the code.



**Unlicensed Code Limitations:**

* **Limited Use:** Only Paulov can use the code. This includes Stay In Tarkov, meaning anyone interacting with, modifying, or distributing the code has violated the license.
* **No Commercial Use:** The code cannot be used for financial gain unless you are Paulov. (e.g., the Ko-Fis in funding.md). 

## LEGAL NOTICE: I AM NOT A LAWYER. I am well versed in copyright law, and have a lawyer in the family, but i am not a lawyer, and i am certintly not anyones lawyer on GitHub. 


<a name="readme-top"></a>

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/stayintarkov/StayInTarkov.Client">
    <img src="Assets/sit-logo-5.png" alt="Logo" height="240">
  </a>

  [![Contributors][contributors-shield]][contributors-url]
  [![Forks][forks-shield]][forks-url]
  [![Stargazers][stars-shield]][stars-url]
  <br/>
  ![TotalDownloads][downloads-total-shield]
  ![LatestDownloads][downloads-latest-shield]


<h3 align="center">Stay In Tarkov Client</h3>

  <p align="center">
    An Escape From Tarkov BepInEx module designed to be used with the SIT.Aki-Server-Mod with the ultimate goal of "Offline" Coop
    <br />
    <a href="https://stayintarkov.com/docs"><strong>Explore the docs »</strong></a>
  </p>

  [English](README.md) **|** [简体中文](README_CN.md) **|** [Deutsch](README_DE.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md) **|** [한국어-Korean](README_KO.md) **|** [Français](README_FR.md)
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
    </li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

Stay In Tarkov (SIT) is a Escape From Tarkov mod designed for cooperative play.

Stay In Tarkov is currently under development by a small team of developers. While SIT is playable, there are many bugs, synchronization and performance issues during gameplay. Escape From Tarkov and SPT-AKI updates frequently and oftentimes, your progress must be reset.

In other words, SIT is not a perfect replacement of the live experience: always keep in mind that game breaking bugs and progression issues will occur and there are no way around it.


<!-- GETTING STARTED -->
## Getting Started

All the information you'll need to get SIT setup can be found on our docs [here](https://stayintarkov.com/docs)

Please make sure you have read the docs before coming to us in the Discord as they likely contain the answers you're looking for


<!-- ROADMAP -->
## Roadmap
Our roadmap can be found [here](https://docs.stayintarkov.com/en/plans.html)

<!-- CONTACT -->
## Contact/Support

The best place to get in contact with us is likely on the SIT Discord.\
Our Discord invite link can be found at https://stayintarkov.com/discord


<!-- CONTRIBUTING -->
## Contributing

* Pull requests are encouraged and deeply appreciated. Thanks to all contributors!

* Code contributions have a strict NO GCLASSXXX policy. If your code has a GCLASS that is neccessary to it working please provide the Pull Request with the list so that they can be remapped before merging.


<!-- LICENSE -->
## License

* 99% of the original core and single-player functionality completed by SPT-Aki teams. There are licenses pertaining to them within this source

* Paulov's work is unlicensed. Unlicensed does not allow any unauthorized or commericial use of Paulov's work. Credit must be provided.

* SIT team's work is MIT licensed

* [RevenantX LiteNetLib](https://github.com/RevenantX/LiteNetLib) is MIT licensed

* [DrakiaXYZ](https://github.com/DrakiaXYZ/) projects contain the MIT License (as of 1.10, Drakia's projects are no longer embedded)



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments & Thanks

* [Paulov Ko-Fi Donations](https://ko-fi.com/paulovt) (original creator of Stay in Tarkov)
* [Mihai Ko-Fi Donations](https://ko-fi.com/mmihai)
* [Trippy](https://github.com/trippyone)
* [Bullet](https://github.com/devbence)
* [Dounai](https://github.com/dounai2333)
* [SPT-Aki team](https://www.sp-tarkov.com/) (Credits provided on each code file used and much love to their Dev team for their support)
* [DrakiaXYZ](https://github.com/DrakiaXYZ/)
* [Contributors](https://github.com/stayintarkov/StayInTarkov.Client/graphs/contributors) and the original contributors of Paulov's SIT.Core
* [RevenantX LiteNetLib](https://github.com/RevenantX/LiteNetLib)



<!-- MARKDOWN LINKS & IMAGES -->
[contributors-shield]: https://img.shields.io/github/contributors/stayintarkov/StayInTarkov.Client.svg?style=for-the-badge

[contributors-url]: https://github.com/stayintarkov/StayInTarkov.Client/graphs/contributors

[forks-shield]: https://img.shields.io/github/forks/stayintarkov/StayInTarkov.Client.svg?style=for-the-badge&color=%234c1

[forks-url]: https://github.com/stayintarkov/StayInTarkov.Client/network/members

[stars-shield]: https://img.shields.io/github/stars/stayintarkov/StayInTarkov.Client?style=for-the-badge&color=%234c1

[stars-url]: https://github.com/stayintarkov/StayInTarkov.Client/stargazers

[downloads-total-shield]: https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/total?style=for-the-badge

[downloads-latest-shield]: https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/latest/total?style=for-the-badge
