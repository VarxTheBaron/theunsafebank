# Suverän Bank!

Den bästa banken.

---

## Black-Box Penetrationstest – Studentguide

Den här guiden ger dig en strukturerad metod för att utföra ett **black-box penetrationstest** av Suverän Bank-applikationen via frontenden. Metoden utgår ifrån [OWASP Web Security Testing Guide (WSTG)](https://owasp.org/www-project-web-security-testing-guide/).

> **Black box** innebär att du inte har tillgång till källkoden. Du testar bara det du kan se och interagera med genom webbläsaren eller andra verktyg.

### Förutsättningar

- Du accessar applikationen via dess publika url.
- Du har tillgång till en webbläsare med utvecklarverktyg (F12)
- Verktyg för att skicka HTTP-förfrågningar (t.ex. `curl`, Hoppscotch.io eller VS Code pluginet REST Client)

---

### Fas 1 – Rekognosering (WSTG-INFO)

> **Mål:** Utforska applikationen, kartlägg sidor, formulär och funktioner.

| # | Uppgift | Tips |
|---|---------|------|
| 1.1 | Öppna startsidan och notera alla synliga länkar och knappar. | Vilka sidor kan du nå utan att logga in? |
| 1.2 | Klicka dig igenom hela siten och skriv upp varje URL du hittar. | T.ex. `/bank`, `/bank/Auth/Login`, etc. |
| 1.3 | Öppna **Developer Tools → Network** i webbläsaren. Ladda om sidan och studera alla anrop. | Vilken teknik verkar siten använda? Syns det några intressanta headers? |
| 1.4 | Undersök cookies som sätts. | Öppna **Application → Cookies** i DevTools. |
| 1.5 | Testa att navigera till sökvägar som inte finns (t.ex. `/bank/admin`, `/bank/test`). | Vad händer? Får du ett felmeddelande? Avslöjar det något? |

**Dokumentera:** Gör en enkel site-karta med alla sidor och formulär du hittade.

---

### Fas 2 – Autentisering (WSTG-ATHN)

> **Mål:** Testa inloggning och registrering efter svagheter.

| # | Uppgift | Tips |
|---|---------|------|
| 2.1 | Registrera ett konto. | Är registreringsprocessen bra? Saknas något |
| 2.2 | Testa att registrera ett konto med ett användarnamn som redan finns. | Hur kan detta utnyttjas? |
| 2.3 | Försök logga in med **fel lösenord**. Upprepa flera gånger. | Finns det något att förbättra med detta? |
| 2.4 | Studera inloggningsformulärets HTML-källa (`Visa sidkälla`). | Skickas lösenordet i klartext? Vilket HTTP-verb används? |
| 2.5 | Logga in och studera cookien som sätts. | Vad av värde sparas i cookien/sessionen? |

**Dokumentera:** Lista alla brister du hittade i autentiseringen.

---

### Fas 3 – Auktorisering / Åtkomstkontroll (WSTG-ATHZ)

> **Mål:** Undersök *hur* applikationen vet vem du är, och om du kan lura den.

| # | Uppgift | Tips |
|---|---------|------|
| 3.1 | Utan att vara inloggad, försök surfa direkt till `/bank/Account/Dashboard`. | *Hur* vet appen att du är inloggad eller inte? Öppna **DevTools → Application → Cookies** och undersök vilka cookies som finns (eller saknas). |
| 3.2 | Logga nu in med ett konto. Gå tillbaka till **Cookies** i DevTools. | Vilken cookie dök upp? Vad heter den? Vad har den för värde?|

**Dokumentera:** Lista alla brister du hittade i auktoriseringen.

---

### Fas 4 – Indatavalidering (WSTG-INPV)

> **Mål:** Testa vad som händer när du skickar oväntade eller skadliga värden.

#### 4a – Cross-Site Scripting (XSS)

| # | Uppgift | Tips |
|---|---------|------|
| 4a.1 | Testa olika inputfält för XSS | Påverkas text av html-taggar? |
| 4a.2 | Vilka fält skulle kunna få Javascript att köras hos en annan användare? | Hur skulle du kunna utnyttja detta? |
| 4a.4 | Ta dig en funderare på: Om du kan köra JavaScript i en annan användares webbläsare, vad kan du göra? | Tänk `document.cookie`, omdirigering, keylogging... |

**Dokumentera:* Vilka fält är sårbara för XSS? Vad kan en angripare göra med det?

#### 4b – SQL Injection

| # | Uppgift | Tips |
|---|---------|------|
| 4b.1 | I inloggningsformuläret, testa användarnamn: `' OR 1=1 --` | Loggas du in? |
| 4b.2 | Testa i andra formulärfält. | Var finns det sårbara fält? Vad kan du utnyttja det till? |
| 4b.3 | Observera felmeddelanden noga. | Avslöjar något felmeddelande databas-teknik eller tabell-namn? |

#### 4c – Andra indataproblem

| # | Uppgift | Tips |
|---|---------|------|
| 4c.1 | Gör en överföring med **negativt belopp** (t.ex. `-5000`). | Vad händer med bägge kontons saldo? |
| 4c.2 | Gör en överföring med ett **extremt stort belopp**. | Händer det något oväntat? |
| 4c.3 | Gör en överföring med belopp `0`. | Tillåts det? |
| 4c.4 | Skicka bokstäver istället för siffror i beloppsfältet. | Hur hanteras det? |

**Dokumentera:** Vilka fält saknar ordentlig validering? Vilka attacker lyckades?

---

### Fas 6 – Session Management Testing - CSRF – Cross-Site Request Forgery (WSTG-SESS)

> **Mål:** Kan du lura en inloggad användare att utföra en handling utan deras vetskap?

| # | Uppgift | Tips |
|---|---------|------|
| 5.1 | Studera överföringsformuläret i källkoden. | Finns det ett dolt fält med en anti-CSRF-token? |
| 5.2 | Skapa en **egen HTML-fil** på din dator med ett formulär som automatiskt skickar en överföring till Suverän Bank. Exempelvis: | Se nedan. |
| 5.3 | Logga in i Suverän Bank i en flik. Öppna din onda HTML-fil i en annan flik. | Genomförs överföringen automatiskt? |

Exempelfil för CSRF-attack:

```html
<!-- spara som evil.html och öppna i webbläsaren -->
<html>
<body>
  <h1>Grattis! Du har vunnit 1000 kr!</h1>
  <form id="attack" method="POST"
        action="http://www.suvnet.se/bank/Account/Transfer">
    <input type="hidden" name="toAccountNumber" value="1000002" />
    <input type="hidden" name="amount" value="5000" />
    <input type="hidden" name="receiverMessage" value="CSRF" />
    <input type="hidden" name="senderNote" value="" />
  </form>
  <script>document.getElementById('attack').submit();</script>
</body>
</html>
```

**Dokumentera:** Lyckades attacken? Varför/varför inte?

---

### Fas 6 – Felhantering och informationsläckage (WSTG-ERRH)

> **Mål:** Avslöjar applikationen känslig information genom felmeddelanden?

| # | Uppgift | Tips |
|---|---------|------|
| 6.1 | Provocera fram fel: navigera till ogiltiga sidor, skicka trasig data, etc. | Visas stacktraces eller tekniknamn (ASP.NET, SQLite, etc.)? |
| 6.2 | Studera felmeddelanden vid inloggning. | Skiljer sig meddelandet åt mellan "fel användarnamn" och "fel lösenord"? Det hjälper en angripare kartlägga giltiga konton. |
| 6.3 | Studera kontonummer i transaktionshistoriken. | Är de förutsägbara (t.ex. sekventiella)? Kan du gissa andra kunders kontonummer? |
| 6.4 | Kolla HTTP-headers i svaret (**Network → Response Headers** i DevTools). | Finns det headers som avslöjar serverversion, ramverk, etc.? |

**Dokumentera:** All information du lyckades utvinna utan att vara behörig.

---

### Fas 7 – Sessionshantering (WSTG-SESS)

> **Mål:** Är sessionen säker?

| # | Uppgift | Tips |
|---|---------|------|
| 7.1 | Logga in och kopiera din sessionscookie. | Öppna **DevTools → Application → Cookies**. |
| 7.2 | Öppna ett **privat/incognito-fönster** och sätt in samma cookie manuellt. | Kan du "kapa" sessionen? |
| 7.3 | Kontrollera cookie-flaggorna: `HttpOnly`? `Secure`? `SameSite`? | Vilka saknas, och vad innebär det? |
| 7.4 | Logga ut. Fungerar den gamla cookien fortfarande? | Försvinner sessionen verkligen vid utloggning? |
| 7.5 | Kombinera med XSS: om du kan köra `document.cookie` i en annan användares webbläsare, kan du då stjäla sessionen? | Koppla ihop fynden. |

---

### Fas 8 – Affärslogik (WSTG-BUSL)

> **Mål:** Kan du missbruka applikationens funktioner på sätt som utvecklaren inte tänkt?

| # | Uppgift | Tips |
|---|---------|------|
| 8.1 | Kan du överföra mer pengar än vad du har? | Vad händer om du har 10 000 kr och försöker föra över 15 000 kr? |
| 8.2 | Kan du göra en överföring till ett **obefintligt konto**? | Försvinner pengarna? |
| 8.3 | *(Avancerat)* Skicka flera överföringar **samtidigt** (t.ex. via `curl` i en loop). | Kan du utnyttja en *race condition* och skicka mer pengar än du har? |
| 8.4 | Exportera din data. Testa med andra användares uppgifter i formuläret. | Kan du exportera **någon annans** data? |

---

### Sammanfattning – Rapportmall

Välj ut två eller tre av de mest allvarliga sårbarheterna du hittade och en kort rapport enligt mallen nedan. Använd gärna OWASP-kategorierna för att klassificera varje sårbarhet.

```
## [Namn på sårbarheten]

**OWASP-kategori:** T.ex. WSTG-INPV-02 (XSS)
**Allvarlighetsgrad:** Kritisk / Hög / Medel / Låg
**Sida/Funktion:** T.ex. Överföringsformuläret, meddelandefältet

### Beskrivning
Vad är problemet? Förklara kort.

### Steg för att återskapa
1. Gå till ...
2. Skriv in ...
3. Klicka på ...
4. Observera att ...

### Påverkan
Vad kan en angripare göra med denna sårbarhet?

### Rekommenderad åtgärd
Om du har en aning om hur detta skulle kunna fixas, beskriv det här. Annars kan du lämna det tomt (Vi kommer in på fixar senare).
```

---

### OWASP WSTG-referens

| Kodnummer | Område | Relevanta faser ovan |
|-----------|--------|---------------------|
| WSTG-INFO | Informationsinsamling | Fas 1 |
| WSTG-ATHN | Autentisering | Fas 4 |
| WSTG-ATHZ | Auktorisering | Fas 5|
| WSTG-INPV | Indatavalidering | Fas 7 |
| WSTG-ERRH | Felhantering | Fas 8 |
| WSTG-BUSL | Affärslogik | Fas 10 |

Du hittar mer specifik information på OWASPs hemsida, exemplvis:
https://owasp.org/www-project-web-security-testing-guide/latest/4-Web_Application_Security_Testing/07-Input_Validation_Testing/02-Testing_for_Stored_Cross_Site_Scripting


Mer info: [https://owasp.org/www-project-web-security-testing-guide/](https://owasp.org/www-project-web-security-testing-guide/)