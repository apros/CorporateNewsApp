# CorporateNews

CorporateNews is a web application that provides AI-powered insights into company news and promotions. It uses Semantic Kernel to process user queries and deliver relevant information.

## Table of Contents

- [Features](#features)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
- [Usage](#usage)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)

## Features

- AI-powered chat interface for querying company news and promotions
- Integration with Semantic Kernel for natural language processing
- Real-time news fetching and processing
- Conversation history management
- Responsive web design

## Project Structure

The solution consists of the following projects:

- `CorporateNews.Web`: The main web application (Razor Pages)
- `CorporateNews.WebAPI`: API for fetching news data
- `CorporateNews.Test`: Unit tests for both Web and WebAPI projects

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- An OpenAI API key

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/CorporateNews.git
   ```

2. Navigate to the project directory:
   ```
   cd CorporateNews
   ```

3. Restore the NuGet packages:
   ```
   dotnet restore
   ```

4. Set up your OpenAI API key:
   - Create a `appsettings.Development.json` file in the `CorporateNews.Web` directory
   - Add your API key to the file:
     ```json
     {
       "OpenAI": {
         "ApiKey": "your-api-key-here"
       }
     }
     ```

5. Build the solution:
   ```
   dotnet build
   ```

## Usage

1. Run the Web application:
   ```
   cd CorporateNews.Web
   dotnet run
   ```

2. Run the WebAPI (in a separate terminal):
   ```
   cd CorporateNews.WebAPI
   dotnet run
   ```

3. Open a web browser and navigate to `https://localhost:5001` (or the port specified in the console output)

4. Use the chat interface to ask questions about company news and promotions

## Testing

To run the unit tests:

cd CorporateNews.Test
dotnet test


## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
