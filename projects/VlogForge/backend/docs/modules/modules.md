# Module and Dependency Overview

Related Stories: [Story ID]

## Module Map

- {VlogForge}.Api
- {VlogForge}.Application
- {VlogForge}.Domain
- {VlogForge}.Infrastructure

## Dependency Rules

- Api depends on Application and Infrastructure
- Application depends on Domain
- Domain has no dependencies
- Infrastructure depends on Domain and Application

