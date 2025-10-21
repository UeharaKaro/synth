# Contributing to Synth

First off, thank you for considering contributing to Synth! It's people like you that make Synth such a great rhythm game.

## Code of Conduct

This project and everyone participating in it is governed by respect and professionalism. By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When creating a bug report, include as many details as possible:

**Bug Report Template:**
- **Description**: Clear description of the bug
- **Steps to Reproduce**: Numbered steps to reproduce the behavior
- **Expected Behavior**: What you expected to happen
- **Actual Behavior**: What actually happened
- **Screenshots**: If applicable
- **Environment**:
  - Unity Version
  - OS and Version
  - Hardware specs (if performance-related)

### Suggesting Features

Feature suggestions are tracked as GitHub issues. When suggesting a feature:

- **Use a clear title** that describes the suggestion
- **Provide detailed description** of the suggested feature
- **Explain why this feature would be useful** to most users
- **List any alternatives** you've considered

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding style** used throughout the project
3. **Write clear commit messages** following our commit conventions
4. **Test your changes** thoroughly
5. **Update documentation** if needed
6. **Submit a pull request** with a clear description

#### Pull Request Process

1. Update the README.md or relevant documentation with details of changes
2. Ensure all tests pass and the project builds successfully
3. Your PR will be reviewed by maintainers
4. Address any requested changes
5. Once approved, your PR will be merged

## Development Setup

### Prerequisites

- Unity 2021.3 or higher
- Git
- A code editor (Visual Studio, Rider, or VS Code recommended)

### Getting Started

1. **Clone your fork**
   ```bash
   git clone https://github.com/your-username/synth.git
   cd synth
   ```

2. **Create a branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Open in Unity**
   - Open Unity Hub
   - Add the `Synth` folder
   - Open with Unity 2021.3+

4. **Make your changes**
   - Write clean, documented code
   - Follow C# naming conventions
   - Add comments for complex logic

5. **Test thoroughly**
   - Test in Unity Editor
   - Test in build if applicable
   - Ensure no console errors

6. **Commit your changes**
   ```bash
   git add .
   git commit -m "Add: Your descriptive commit message"
   ```

7. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

8. **Create Pull Request**
   - Go to the original repository
   - Click "New Pull Request"
   - Select your fork and branch
   - Fill out the PR template

## Coding Standards

### C# Style Guide

- **Naming Conventions**:
  - Classes: `PascalCase` (e.g., `SongSelectionUI`)
  - Methods: `PascalCase` (e.g., `LoadSong()`)
  - Private fields: `camelCase` (e.g., `currentSongIndex`)
  - Public fields/properties: `PascalCase`
  - Constants: `UPPER_CASE` or `PascalCase`

- **Documentation**:
  - Add XML comments to public methods
  - Use `/// <summary>` tags
  - Document complex algorithms

- **Code Organization**:
  - One class per file
  - Group related functionality
  - Keep methods focused and small

### Unity Specific

- **Serialized Fields**:
  ```csharp
  [Header("Section Name")]
  [Tooltip("Description")]
  public Type fieldName;
  ```

- **ScriptableObjects**:
  - Use for data management
  - Add `CreateAssetMenu` attribute

- **Prefabs**:
  - Keep prefabs modular
  - Document component requirements

## Commit Message Guidelines

Follow this format for commit messages:

```
Type: Short description (50 chars or less)

Longer description if necessary, explaining what changed and why.
Include references to issues if applicable.

Fixes #123
```

**Types:**
- `Add`: New feature or file
- `Update`: Changes to existing functionality
- `Fix`: Bug fixes
- `Remove`: Removing code or files
- `Refactor`: Code restructuring without feature changes
- `Docs`: Documentation changes
- `Style`: Formatting, missing semicolons, etc.
- `Test`: Adding or updating tests

**Examples:**
```
Add: Song search functionality in selection UI

Update: Improve judgment timing accuracy

Fix: Prevent crash when no songs in database

Docs: Add setup instructions to README
```

## Project Structure

When adding new features, follow this structure:

```
Synth/Assets/
├── [feature]/          # Feature folder
│   ├── *.cs           # Scripts
│   ├── README.md      # Feature documentation
│   └── ...
```

## Testing

- **Manual Testing**: Test your changes in Unity Editor
- **Build Testing**: Test in actual builds when possible
- **Edge Cases**: Consider edge cases and error handling
- **Performance**: Be mindful of performance implications

## Documentation

When adding features:

1. **Update README.md** if it affects installation or usage
2. **Create feature README** in the feature folder
3. **Add code comments** for complex logic
4. **Update API documentation** if applicable

## Questions?

Feel free to ask questions by:
- Opening an issue with the "question" label
- Reaching out to maintainers

## Recognition

Contributors will be recognized in:
- README.md acknowledgments section
- Release notes for significant contributions

Thank you for contributing to Synth! 🎵
