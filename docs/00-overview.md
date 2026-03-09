# Web API 200 - Applied APIs

## Course Overview

This course follows the 100 course. In the 100 course we learn the "A,B,C's" of API Development with .NET.

Some Key Points From 100:

- .NET, .NET Core
- Hosting APIs ("Port Binding" from the Twelve Factor App)
- Creating a Development Environment
  - We used Aspire
- Implementing HTTP
  - With Controllers (the Vendors API)
  - With "Minimal APIs" (the Catalog Items API)
- AuthN/AuthZ
  - Bearer Tokens
  - IDP
- Developer Testing
  - Mostly focused on "System Tests"


## Review of the Code from the Last 100

*note* I added some stuff here, so pay attention.

- More tests.
- The Catalog Items "Fleshed Out"


## Critique

- Let's evaluate what we have and come up with a list of "smells".
- Look for:
  - Code Duplication 
  - Anything that keeps us from a real "System Test"
  - Performance Issues

## More Advanced API "Stuff"

- Middleware and Filters
  - Action Filters on Controllers
  - Endpoint Filters on Minimal APIs
- Options Pattern
- Using Test Doubles for:
  - Databases
  - APIs
  - Etc.
- Background Work
  - Transactional Outbox / Inbox
  

  ## Reliability, Messaging




