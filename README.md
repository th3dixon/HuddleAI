# HuddleAI - AI-Powered Sports Performance Analysis

A comprehensive sports analytics platform built with ASP.NET Core 9 that provides AI-powered performance analysis for athletes through video and image analysis using Google Gemini 1.5 Flash.

## 🚀 Features

- **AI Sports Analysis**: Upload videos or images of athletic performance and get detailed AI analysis
- **Structured Feedback**: Receive overall performance scores (1-100), detailed overviews, improvement areas, and actionable plans
- **Modern UI**: Built with Tailwind CSS and Alpine.js for a responsive, social media-inspired design
- **Real-time Upload**: AJAX file upload with progress indicators and drag-and-drop support
- **Multi-Sport Support**: Analyze performance across various sports with customizable focus areas
- **RESTful API**: Clean API architecture with proper separation of concerns

## 🏗️ Architecture

- **Framework**: ASP.NET Core 9.0 with C# 12
- **Frontend**: ASP.NET Core MVC with Tailwind CSS and Alpine.js
- **AI Integration**: Google Gemini 1.5 Flash API for performance analysis
- **Database**: Entity Framework Core with In-Memory database (prototype)
- **Architecture**: N-tier architecture with API and Web layers

## 🛠️ Project Structure

```
HuddleAI/
├── src/
│   ├── API/HuddleAI.API/              # Backend API services
│   │   ├── Controllers/               # API endpoints
│   │   ├── Services/                  # Business logic (Gemini integration)
│   │   ├── Data/                      # Entity Framework DbContext
│   │   └── Entities/                  # Domain models
│   ├── Web/HuddleAI.WebApp/          # Frontend MVC application
│   └── BuildingBlocks/
│       └── HuddleAI.SharedKernel/     # Shared DTOs and utilities
└── tests/                             # Test projects (future)
```

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Google Gemini API key

### Running the Application

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/HuddleAI.git
   cd HuddleAI
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Start the API** (Terminal 1)
   ```bash
   dotnet run --project src/API/HuddleAI.API
   ```
   API will be available at: `https://localhost:7010`

4. **Start the Web App** (Terminal 2)
   ```bash
   dotnet run --project src/Web/HuddleAI.WebApp
   ```
   Web app will be available at: `https://localhost:56680`

5. **Access the application**
   - Open your browser and navigate to `https://localhost:56680`
   - Upload a sports video or image
   - Enter the sport and analysis focus
   - Get AI-powered performance analysis!

## 📝 API Configuration

The Gemini API key is configured in `src/API/HuddleAI.API/appsettings.json`:

```json
{
  "GeminiApiKey": "your-api-key-here"
}
```

## 🎯 Usage

1. **Upload Media**: Drag and drop or select a video/image file
2. **Specify Sport**: Enter the sport being analyzed (e.g., "Basketball", "Tennis")
3. **Analysis Focus**: Describe what to analyze (e.g., "Shooting technique", "Serving motion")
4. **Get Results**: Receive structured analysis with:
   - Overall performance score (1-100)
   - Detailed performance overview
   - Specific areas for improvement
   - Actionable improvement plan

## 🔧 Supported File Types

- **Videos**: MP4, AVI, MOV
- **Images**: JPG, JPEG, PNG, GIF
- **Size Limit**: 50MB per file

## 🎨 Design System

Built with Tailwind CSS using a modern, social media-inspired design with:

- **Primary**: Blue (#3B82F6)
- **Secondary**: Green (#10B981)
- **Accent**: Amber (#F59E0B)
- **Success**: Emerald (#06D6A0)
- **Warm**: Red (#FF6B6B)

## 🔮 Future Enhancements

- Persistent database with SQL Server
- User authentication and profiles
- Analysis history and progress tracking
- Social features and community sharing
- Advanced analytics and reporting
- Mobile app development

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🙋‍♂️ Support

For questions or support, please open an issue in the GitHub repository.

---

Built with ❤️ using .NET 9 and Google Gemini AI