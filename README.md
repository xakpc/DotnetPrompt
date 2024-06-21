![Image](./docs/images/logo.png)

## Quick Start

DotnetPrompt is a dotnet library that provides tools for working with Large Language Models (LLMs) individually and combined in chains.
Our library helps you integrate LLMs with other tools and resources to create powerful AI applications. 

The concept of the library has failed (semantic whatever from MS does it better anyway), so I'm looking in what direction to migrate it

## Important Milestones

Need to review where that could go

- [ ] Get an abstraction atop commercial LLMs (with ChatGPT APIs)
- [ ] Provide the ability to store inputs/outputs to decouple yourself from the platform and own your data
- [ ] Be a drag&drop replacement for MS OpenAI client - but with adapters to other models/platforms
- [ ] Keep only core components
    - A client for LLM communication
    - Functions/Tools for function calling 
    - A vector database for RAG
    - An Observability platform for tracing, evaluation etc.
