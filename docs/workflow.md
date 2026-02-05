# 🔄 Working Agreements & Workflow

### 🌿 Branching Strategy
- *main*: Protected branch. Only stable code.
- *feature/CI-[Ticket#]*: Use for all new features (e.g., feature/CI-101-auth).
- *bugfix/CI-[Ticket#]*: Use for fixing bugs.

### 📥 Pull Request (PR) Workflow
1. *Push branch* to origin.
2. *Open PR* against main.
3. *Peer Review*: At least one approval required.
4. *Validation*: Ensure dotnet build passes with no errors.
5. *Merge*: Use 'Squash and Merge' to keep history clean.

### ✅ Definition of Done (DoD)
- [ ] Code follows the Clean Architecture layers (Domain → Application → Infrastructure).
- [ ] Dependency Injection is registered in Program.cs.
- [ ] No hardcoded strings; use configuration files.
- [ ] API successfully communicates with the Angular UI.


