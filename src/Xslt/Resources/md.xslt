<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:txt="http://kampute.com/doc-tools/transform/text"
  xmlns:md="http://kampute.com/doc-tools/transform/markdown"
  xmlns:doc="http://kampute.com/doc-tools/transform/xml-doc"
  exclude-result-prefixes="txt doc">

  <xsl:output method="text" indent="no"/>
  <xsl:strip-space elements="*"/>

  <!-- Template to match the root node and apply templates to its children -->
  <xsl:template match="/">
    <xsl:apply-templates/>
  </xsl:template>

  <!-- Template to handle text nodes -->
  <xsl:template match="text()">
    <xsl:value-of select="md:Escape(txt:NormalizeWhitespace(.))"/>
  </xsl:template>

  <!-- Template to handle 'para' elements, adding empty lines before and/or after -->
  <xsl:template match="para">
    <xsl:if test="preceding-sibling::para or (preceding-sibling::node()[not(self::text()[normalize-space()=''])])">
      <xsl:text xml:space="preserve">&#10;&#10;</xsl:text>
    </xsl:if>
    <xsl:apply-templates/>
    <xsl:if test="following-sibling::node()[not(self::para) and not(self::text()[normalize-space()=''])]">
      <xsl:text xml:space="preserve">&#10;&#10;</xsl:text>
    </xsl:if>
  </xsl:template>

  <!-- Template to handle 'c' elements, formatting content as inline code -->
  <xsl:template match="c">
    <xsl:text>`</xsl:text>
    <xsl:apply-templates/>
    <xsl:text>`</xsl:text>
  </xsl:template>

  <!-- Template to handle 'code' elements, formatting content as code block -->
  <xsl:template match="code">
    <xsl:variable xml:space="preserve" name="code" select="text()"/>
    <xsl:variable name="fence" select="md:FenceMarker($code)"/>
    <xsl:text xml:space="preserve">&#10;</xsl:text>
    <xsl:value-of select="$fence"/>
    <xsl:choose>
      <xsl:when test="@language">
        <xsl:value-of select="@language"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="doc:GetLanguageId($code)"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text xml:space="preserve">&#10;</xsl:text>
    <xsl:value-of xml:space="preserve" select="doc:FormatCode($code)"/>
    <xsl:text xml:space="preserve">&#10;</xsl:text>
    <xsl:value-of select="$fence"/>
    <xsl:text xml:space="preserve">&#10;</xsl:text>
  </xsl:template>

  <!-- Template to handle 'paramref' and 'typeparamref' elements, formatting content as inline code -->
  <xsl:template match="paramref|typeparamref">
    <xsl:text>`</xsl:text>
    <xsl:value-of select="md:EscapeInline(@name)"/>
    <xsl:text>`</xsl:text>
  </xsl:template>

  <!-- Template to handle 'see' elements, formatting them as links -->
  <xsl:template match="see">
    <xsl:choose>
      <!-- Handle 'see' elements with 'langword' attribute -->
      <xsl:when test="@langword">
        <xsl:call-template name="render-link">
          <xsl:with-param name="url" select="doc:GetKeywordUrl(@langword)"/>
          <xsl:with-param name="text">
            <xsl:text>`</xsl:text>
            <xsl:value-of select="md:EscapeInline(@langword)"/>
            <xsl:text>`</xsl:text>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <!-- Handle 'see' elements with 'cref' attribute -->
      <xsl:when test="@cref">
        <xsl:call-template name="render-link">
          <xsl:with-param name="url" select="doc:GetCodeReferenceUrl(@cref)"/>
          <xsl:with-param name="text">
            <xsl:choose>
              <xsl:when test="string-length(text()) > 0">
                <xsl:value-of select="text()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:variable name="title" select="doc:GetCodeReferenceTitle(@cref)"/>
                <xsl:choose>
                  <xsl:when test="string-length($title) > 0">
                    <xsl:value-of select="md:EscapeInline($title)"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="md:EscapeInline(@cref)"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <!-- Handle 'see' elements with 'href' attribute -->
      <xsl:when test="@href">
        <xsl:call-template name="render-link">
          <xsl:with-param name="url" select="doc:GetTopicUrl(@href)"/>
          <xsl:with-param name="text">
            <xsl:choose>
              <xsl:when test="string-length(text()) > 0">
                <xsl:value-of select="text()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:variable name="title" select="doc:GetTopicTitle(@href)"/>
                <xsl:choose>
                  <xsl:when test="string-length($title) > 0">
                    <xsl:value-of select="md:EscapeInline($title)"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="md:EscapeInline(@href)"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Helper template for rendering links -->
  <xsl:template name="render-link">
    <xsl:param name="url"/>
    <xsl:param name="text"/>
    <xsl:choose>
      <xsl:when test="string-length($url) > 0">
        <xsl:text>[</xsl:text>
        <xsl:value-of select="$text"/>
        <xsl:text>](</xsl:text>
        <xsl:value-of select="md:EscapeInline($url)"/>
        <xsl:text>)</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Template to handle 'list' elements, formatting them as tables or lists -->
  <xsl:template match="list">
    <xsl:text xml:space="preserve">&#10;&#10;</xsl:text>
    <xsl:choose>
      <!-- Handle 'list' elements with 'type' attribute set to 'table' -->
      <xsl:when test="@type='table'">
        <xsl:for-each select="listheader">
          <xsl:variable name="separator">
            <xsl:for-each select="term|description">
              <xsl:text>|---</xsl:text>
            </xsl:for-each>
            <xsl:text xml:space="preserve">|&#10;</xsl:text>
          </xsl:variable>
          <xsl:for-each select="term|description">
            <xsl:text>|</xsl:text>
            <xsl:apply-templates/>
          </xsl:for-each>
          <xsl:text xml:space="preserve">|&#10;</xsl:text>
          <xsl:value-of xml:space="preserve" select="$separator"/>
        </xsl:for-each>
        <xsl:for-each select="item">
          <xsl:for-each select="term|description">
            <xsl:text>|</xsl:text>
            <xsl:apply-templates/>
          </xsl:for-each>
          <xsl:text xml:space="preserve">|&#10;</xsl:text>
        </xsl:for-each>
      </xsl:when>
      <!-- Handle 'list' elements with 'type' attribute set to 'number' -->
      <xsl:when test="@type='number'">
        <xsl:call-template name="render-list-items">
          <xsl:with-param name="is-numbered" select="true()"/>
        </xsl:call-template>
      </xsl:when>
      <!-- Handle all other 'list' elements -->
      <xsl:otherwise>
        <xsl:call-template name="render-list-items">
          <xsl:with-param name="is-numbered" select="false()"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text xml:space="preserve">&#10;</xsl:text>
  </xsl:template>

  <!-- Helper template to render list items -->
  <xsl:template name="render-list-items">
    <xsl:param name="is-numbered"/>
    <xsl:for-each select="listheader|item">
      <xsl:choose>
        <xsl:when test="$is-numbered">
          <xsl:number value="position()" format="1"/>
          <xsl:text xml:space="preserve">. </xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text xml:space="preserve">- </xsl:text>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="term">
        <xsl:text xml:space="preserve">**</xsl:text>
        <xsl:apply-templates select="term"/>
        <xsl:text xml:space="preserve">** &#8211; </xsl:text>
      </xsl:if>
      <xsl:variable name="content">
        <xsl:choose>
          <xsl:when test="description">
            <xsl:apply-templates select="description"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:value-of xml:space="preserve" select="txt:Replace(txt:TrimEnd($content), '&#10;', '&#10;  ')"/>
      <xsl:text xml:space="preserve">&#10;</xsl:text>
    </xsl:for-each>
  </xsl:template>

  <!-- Template to handle 'note' elements, formatting them as blockquotes -->
  <xsl:template match="note">
    <xsl:text xml:space="preserve">&#10;&#10;</xsl:text>
    <xsl:if test="@title">
      <xsl:text xml:space="preserve">&gt; **</xsl:text>
      <xsl:value-of select="md:EscapeInline(@title)"/>
      <xsl:text xml:space="preserve">** \&#10;</xsl:text>
    </xsl:if>
    <xsl:variable name="content">
      <xsl:apply-templates/>
    </xsl:variable>
    <xsl:text xml:space="preserve">&gt; </xsl:text>
    <xsl:value-of xml:space="preserve" select="txt:Replace(txt:TrimEnd($content), '&#10;', '&#10;&gt; ')"/>
    <xsl:text xml:space="preserve">&#10;&#10;</xsl:text>
  </xsl:template>

</xsl:stylesheet>